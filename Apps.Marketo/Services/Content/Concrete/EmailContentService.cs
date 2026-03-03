using Apps.Marketo.Constants;
using Apps.Marketo.Dtos;
using Apps.Marketo.Dtos.Content;
using Apps.Marketo.Extensions;
using Apps.Marketo.Helper.FileFolder;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.HtmlHelpers;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Content.Response;
using Apps.Marketo.Models.Entities;
using Apps.Marketo.Models.Entities.Email;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Mime;
using System.Text;

namespace Apps.Marketo.Services.Content.Concrete;

public class EmailContentService(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext), IContentService
{
    public async Task<FileReference> DownloadContent(DownloadContentRequest input)
    {
        var emailInfoRequest = new RestRequest($"/rest/asset/v1/email/{input.ContentId}.json", Method.Get);
        var emailInfo = await Client.ExecuteWithErrorHandlingFirst<EmailEntity>(emailInfoRequest);

        var emailContentRequest = new RestRequest($"/rest/asset/v1/email/{input.ContentId}/content.json", Method.Get);
        var emailContentResponse = await Client.ExecuteWithErrorHandling<EmailContentDto>(emailContentRequest);

        bool onlyDynamic = input.GetOnlyDynamicContent ?? false;
        bool includeImages = input.IncludeImages ?? false;

        var contentTasks = emailContentResponse
            .Where(x => x.ContentType == "DynamicContent"
                     || (!onlyDynamic && x.ContentType == "Text")
                     || (includeImages && x.ContentType == "Image"))
            .Select(async item =>
            {
                var content = await GetEmailSectionContent(
                    input.ContentId,
                    input.Segment,
                    item,
                    includeImages);

                return new KeyValuePair<string, string?>(item.HtmlId, content);
            });

        var contentResults = await Task.WhenAll(contentTasks);

        var sectionContent = contentResults
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!);

        if (emailInfo.Subject?.Type == "Text" && !string.IsNullOrEmpty(emailInfo.Subject.Value))
            sectionContent.Add("data-subject-value", emailInfo.Subject.Value);
        else if (emailInfo.Subject?.Type == "DynamicContent")
        {
            var subjectContent = await GetEmailSectionContent(
                input.ContentId,
                input.Segment,
                new EmailContentDto
                {
                    ContentType = "DynamicContent",
                    Value = emailInfo.Subject.Value
                },
                includeImages);

            if (!string.IsNullOrEmpty(subjectContent))
                sectionContent.Add("data-subject-value", subjectContent);
        }

        var metadata = new List<MetadataEntity> { new(MetadataConstants.BlackbirdEmailId, input.ContentId) };

        if (emailContentResponse.Any(x => x.ContentType == "DynamicContent") || emailInfo.Subject?.Type == "DynamicContent")
        {
            var resolvedSegmentName = string.IsNullOrWhiteSpace(input.Segment) ? "Default" : input.Segment;
            metadata.Add(new(MetadataConstants.BlackbirdSegmentName, resolvedSegmentName));

            if (!string.IsNullOrWhiteSpace(input.SegmentationId))
                metadata.Add(new(MetadataConstants.BlackbirdSegmentationId, input.SegmentationId));
        }

        var resultHtml = HtmlContentBuilder.GenerateHtml(sectionContent, emailInfo.Name, metadata);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        return await fileManagementClient.UploadAsync(stream, MediaTypeNames.Text.Html, emailInfo.Name.ToHtmlFileName());
    }

    public async Task<SearchContentResponse> SearchContent(SearchContentRequest input)
    {
        var request = new RestRequest($"/rest/asset/v1/emails.json", Method.Get);
        var subfolders = await FileFolderHelper.AddFolderParameter(Client, request, input.FolderId);
        request
            .AddQueryParameterIfNotNull("status", input.Status)
            .AddQueryParameterIfNotNull("earliestUpdatedAt", input.UpdatedAfter)
            .AddQueryParameterIfNotNull("latestUpdatedAt", input.UpdatedBefore);

        var emails = await Client.Paginate<EmailEntity>(request);

        emails = emails
            .ApplyDateAfterFilter(input.CreatedAfter, x => x.CreatedAt)
            .ApplyDateBeforeFilter(input.CreatedBefore, x => x.CreatedAt)
            .ApplyNamePatternFilter(input.NamePatterns, input.ExcludeMatched, x => x.Name);

        emails = await emails.ApplyIgnoreInArchiveFilter(Client, input.IgnoreInArchive, x => x.Folder);

        return new(emails.Select(x => new ContentDto(x)).ToList());
    }

    private async Task<string?> GetEmailSectionContent(
        string emailId,
        string? segmentName,
        EmailContentDto sectionContent,
        bool includeImages)
    {
        if (sectionContent.ContentType == "Text")
        {
            var textValues = JsonConvert.DeserializeObject<List<EmailContentValueDto>>(sectionContent.Value.ToString()!);
            return textValues?.FirstOrDefault(x => x.Type == "HTML")?.Value ?? string.Empty;
        }

        if (sectionContent.ContentType == "Image" && includeImages)
        {
            var imageDto = JsonConvert.DeserializeObject<ImageDto>(sectionContent.Value.ToString()!);
            var imageUrl = string.IsNullOrWhiteSpace(imageDto!.ContentUrl) ? imageDto.Value : imageDto.ContentUrl;

            return BuildImageTag(imageUrl, imageDto.Style, imageDto.AltText, null, null);
        }

        if (sectionContent.ContentType == "DynamicContent")
        {
            var requestSeg = new RestRequest($"/rest/asset/v1/email/{emailId}/dynamicContent/{sectionContent.Value}.json", Method.Get);
            var responseSeg = await Client.ExecuteWithErrorHandlingFirst<DynamicContentDto<EmailImageSegmentDto>>(requestSeg);

            var targetSegmentName = string.IsNullOrWhiteSpace(segmentName) ? "Default" : segmentName;

            var targetSegment = responseSeg.Content.FirstOrDefault(x => x.SegmentName == targetSegmentName);
            if (targetSegment == null) return string.Empty;

            return targetSegment.Type switch
            {
                "Text" or "HTML" => targetSegment.Content,
                "File" when includeImages => BuildImageTag(targetSegment.ContentUrl, targetSegment.Style, targetSegment.AltText, targetSegment.Width, targetSegment.Height, targetSegment.Content),
                "Image" when includeImages => BuildImageTag(targetSegment.Content, targetSegment.Style, targetSegment.AltText, targetSegment.Width, targetSegment.Height, targetSegment.Content),
                _ => string.Empty
            };
        }

        return string.Empty;
    }

    private static string BuildImageTag(
        string src, 
        string? style, 
        string? alt, 
        string? width, 
        string? height, 
        string? contentId = null)
    {
        var altAttr = string.IsNullOrWhiteSpace(alt) ? string.Empty : $" alt=\"{alt}\"";
        var widthAttr = string.IsNullOrWhiteSpace(width) ? string.Empty : $" width=\"{width}\"";
        var heightAttr = string.IsNullOrWhiteSpace(height) ? string.Empty : $" height=\"{height}\"";
        var styleAttr = string.IsNullOrWhiteSpace(style) ? string.Empty : $" style=\"{style}\"";
        var idAttr = 
            string.IsNullOrWhiteSpace(contentId) ? 
            string.Empty : 
            $" {MetadataConstants.ContextImage}=\"{contentId}\"";

        return $"<img src=\"{src}\"{styleAttr}{altAttr}{idAttr}{widthAttr}{heightAttr}>";
    }
}
