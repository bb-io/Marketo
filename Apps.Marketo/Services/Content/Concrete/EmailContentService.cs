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
using Apps.Marketo.Services.Content.Models;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Mime;
using System.Text;

namespace Apps.Marketo.Services.Content.Concrete;

public class EmailContentService(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext), IContentService
{
    public async Task UploadContent(UploadContentInput input)
    {
        string emailId =
            input.ContentId ??
            HtmlContentBuilder.ExtractMeta(input.HtmlContent, MetadataConstants.BlackbirdEmailId) ??
            throw new PluginMisconfigurationException(
                "Email ID is not not found in the input file. Please provide it in the optional input"
                );
        string segmentationId =
            input.SegmentationId ??
            HtmlContentBuilder.ExtractMeta(input.HtmlContent, MetadataConstants.BlackbirdSegmentationId) ??
            throw new PluginMisconfigurationException(
                "Segmentation ID is not not found in the input file. Please provide it in the optional input"
                );
        string segment =
            input.Segment ??
            HtmlContentBuilder.ExtractMeta(input.HtmlContent, MetadataConstants.BlackbirdSegmentName) ??
            throw new PluginMisconfigurationException(
                "Segment name is not not found in the input file. Please provide it in the optional input"
                );

        var translatedContent = HtmlContentBuilder.ParseHtml(input.HtmlContent);

        var emailContentRequest = new RestRequest($"/rest/asset/v1/email/{input.ContentId}/content.json", Method.Get);
        var emailContentResponse = await Client.ExecuteWithErrorHandling<EmailContentDto>(emailContentRequest);

        if (input.UploadOnlyDynamicContent == false)
        {
            foreach (var item in emailContentResponse)
            {
                if (item.ContentType == "Text")
                    await ConvertSectionToDynamicContent(emailId, item.HtmlId, segmentationId);
            }
            emailContentResponse = await Client.ExecuteWithErrorHandling<EmailContentDto>(emailContentRequest);
        }

        if (translatedContent.TryGetValue("data-subject-value", out var subjectContent) && input.UpdateEmailSubject == true)
        {
            var emailInfoRequest = new RestRequest($"/rest/asset/v1/email/{input.ContentId}.json", Method.Post);
            var emailInfo = await Client.ExecuteWithErrorHandlingFirst<EmailEntity>(emailInfoRequest);
            if (emailInfo.Subject?.Type == "Text" && input.UploadOnlyDynamicContent == false)
            {
                var subjectContentRequest = new RestRequest($"/rest/asset/v1/email/{input.ContentId}/content.json", Method.Post)
                    .AddParameter("subject", JsonConvert.SerializeObject(new
                    {
                        type = "Text",
                        value = subjectContent
                    }));

                await Client.ExecuteWithErrorHandlingFirst<IdDto>(subjectContentRequest);
            }
            else if (emailInfo.Subject?.Type == "DynamicContent")
            {
                var errorMessage = await UpdateEmailDynamicContent(emailId, segmentationId, segment, new EmailContentDto()
                {
                    ContentType = "Text",
                    Value = emailInfo.Subject.Value
                }, subjectContent, emailContentResponse.ToList(), translatedContent, 0, true, "Text");
            }
        }

        var modulesToIgnore = new List<string>();
        foreach (var item in emailContentResponse)
        {
            if (item.ContentType == "DynamicContent" &&
                !modulesToIgnore.Contains(item.ParentHtmlId) &&
                translatedContent.TryGetValue(item.HtmlId, out var translatedContentItem))
            {
                var ignoreModule = await UpdateEmailDynamicContent(
                    emailId, 
                    segmentationId, 
                    segment, 
                    item,
                    translatedContentItem, 
                    emailContentResponse.ToList(), 
                    translatedContent, 
                    0,
                    input.RecreateCorruptedEmailModules ?? true,
                    updateStyle: input.UpdateStyleForEmailImages ?? false);
                modulesToIgnore.Add(ignoreModule);
            }
        }
    }

    private async Task<string> UpdateEmailDynamicContent(
        string emailId,
        string segmentationId,
        string segmentName,
        EmailContentDto dynamicContentItem,
        string content,
        List<EmailContentDto> emailContentItems,
        Dictionary<string, string> translatedContent,
        int tryNumber,
        bool reacreateCorruptedModules,
        string? type = null,
        bool updateStyle = false)
    {
        var endpoint = $"/rest/asset/v1/email/{emailId}/dynamicContent/{dynamicContentItem.Value}.json";

        RestRequest request;
        if (!content.Contains(MetadataConstants.ContextImage))
        {
            request = new RestRequest(endpoint, Method.Post)
                .AddQueryParameter("segment", segmentName)
                .AddQueryParameter("type", type ?? "HTML")
                .AddQueryParameter("value", content);
        }
        else
        {
            var htmlSnippet = new HtmlDocument();
            htmlSnippet.LoadHtml(content);
            var altTextAttribute = htmlSnippet.DocumentNode.FirstChild.Attributes["alt"];
            var styleAttribute = htmlSnippet.DocumentNode.FirstChild.Attributes["style"];
            var widthAttribute = htmlSnippet.DocumentNode.FirstChild.Attributes["width"];
            var heightAttribute = htmlSnippet.DocumentNode.FirstChild.Attributes["height"];
            var srcAttribute = htmlSnippet.DocumentNode.FirstChild.Attributes["src"];

            if (altTextAttribute == null)
                return string.Empty;
            var imageIdAttribute = htmlSnippet.DocumentNode.FirstChild.Attributes[MetadataConstants.ContextImage];

            request = new RestRequest(endpoint, Method.Post)
                .AddQueryParameter("segment", segmentName)
                .AddQueryParameter("type", "Image")
                .AddQueryParameter("altText", altTextAttribute.Value);

            if (srcAttribute.Value == imageIdAttribute.Value)
                request.AddQueryParameter("externalUrl", srcAttribute.Value.Replace(" ", "+"));
            else
                request.AddQueryParameter("value", imageIdAttribute.Value);


            if (styleAttribute != null && !string.IsNullOrWhiteSpace(styleAttribute.Value) && updateStyle)
            {
                request.AddQueryParameter("style", styleAttribute.Value);
            }

            if (widthAttribute != null && !string.IsNullOrWhiteSpace(widthAttribute.Value))
            {
                if (int.TryParse(widthAttribute.Value, out var width))
                {
                    request.AddQueryParameter("width", width);
                }
            }

            if (heightAttribute != null && !string.IsNullOrWhiteSpace(heightAttribute.Value))
            {
                if (int.TryParse(heightAttribute.Value, out var height))
                {
                    request.AddQueryParameter("height", height);
                }
            }
        }

        var result = await Client.ExecuteNoErrorHandling<IdDto>(request);
        if (result != null)
        {
            if (result.Errors.Any(x => x.Code == "611") && tryNumber == 0 && reacreateCorruptedModules)
            {
                var ignoreModuleId = await RecreateModuleWithIssue(emailId, segmentationId, segmentName, dynamicContentItem, emailContentItems, translatedContent, tryNumber);
                return ignoreModuleId;
            }

            var errorMessages = result.Errors.Select(x => x.Message).ToList();
            string errorMessage = string.Join("; ", errorMessages);
            return $"{errorMessage}, ContentId: {dynamicContentItem.Value.ToString()}, Content: {content}";
        }

        return string.Empty;
    }

    private async Task<string> RecreateModuleWithIssue(
        string emailId,
        string segmentationId,
        string segmentName,
        EmailContentDto dynamicContentItem,
        List<EmailContentDto> emailContentItems,
        Dictionary<string, string> translatedContent,
        int tryNumber)
    {
        var parentModule = emailContentItems.FirstOrDefault(x => x.HtmlId == dynamicContentItem.ParentHtmlId);
        var oldItemsInModule = emailContentItems.Where(x => x.ParentHtmlId == dynamicContentItem.ParentHtmlId).ToList();

        var deleteModuleRequest = new RestRequest($"/rest/asset/v1/email/{emailId}/content/{dynamicContentItem.ParentHtmlId}/delete.json", Method.Post);
        var deleteModuleResponse = await Client.ExecuteWithErrorHandlingFirst<IdDto>(deleteModuleRequest);

        var addNewModuleRequest = new RestRequest($"/rest/asset/v1/email/{emailId}/content/{dynamicContentItem.ParentHtmlId}/add.json", Method.Post);
        addNewModuleRequest.AddQueryParameter("index", parentModule.Index);
        var addNewModuleResponse = await Client.ExecuteWithErrorHandlingFirst<IdDto>(addNewModuleRequest);

        var emailContentRequest = new RestRequest($"/rest/asset/v1/email/{emailId}/content.json", Method.Get);
        var emailContentResponse = await Client.ExecuteWithErrorHandling<EmailContentDto>(emailContentRequest);

        var itemsInNewModule = emailContentResponse.Where(x => !string.IsNullOrEmpty(x.ParentHtmlId) && x.ParentHtmlId.Length > 36 && parentModule.HtmlId.Contains(x.ParentHtmlId.Remove(x.ParentHtmlId.Length - 36, 36))).ToList();
        var textItemsIds = new List<string>();
        foreach (var item in itemsInNewModule)
        {
            var idWithoutGuid = item.HtmlId.Remove(item.HtmlId.Length - 36, 36);
            var oldModuleItem = oldItemsInModule.FirstOrDefault(x => x.HtmlId.Contains(idWithoutGuid));
            if (item.ContentType == "Text" && oldModuleItem != null && oldModuleItem.ContentType == "DynamicContent")
            {
                await ConvertSectionToDynamicContent(emailId, item.HtmlId, segmentationId);
                textItemsIds.Add(item.HtmlId);
            }
            else if (item.ContentType != "DynamicContent" && oldModuleItem.ContentType != "DynamicContent")
            {
                var updateModuleItemRequest = new RestRequest($"/rest/asset/v1/email/{emailId}/content/{item.HtmlId}.json", Method.Post);
                updateModuleItemRequest.AddQueryParameter("type", oldModuleItem.ContentType);
                updateModuleItemRequest.AddQueryParameter("value", oldModuleItem.Value.ToString());
                var updateModuleItemResponse = await Client.ExecuteWithErrorHandlingFirst<IdDto>(updateModuleItemRequest);
            }
        }

        emailContentResponse = await Client.ExecuteWithErrorHandling<EmailContentDto>(emailContentRequest);
        itemsInNewModule = emailContentResponse.Where(x => textItemsIds.Contains(x.HtmlId)).ToList();

        foreach (var dynamicItem in itemsInNewModule)
        {
            var idWithoutGuid = dynamicItem.HtmlId.Remove(dynamicItem.HtmlId.Length - 36, 36);
            if (translatedContent.TryGetValue(translatedContent.Keys.FirstOrDefault(x => x.Contains(idWithoutGuid)) ?? "", out var translatedContentItem))
            {
                await UpdateEmailDynamicContent(emailId, segmentationId, segmentName, dynamicItem, translatedContentItem, null, null, ++tryNumber, false);
            }
        }
        return parentModule.HtmlId;
    }

    private async Task<IdDto> ConvertSectionToDynamicContent(string emailId, string htmlId, string segmentationId)
    {
        var endpoint = $"/rest/asset/v1/email/{emailId}/content/{htmlId}.json";
        var request = new RestRequest(endpoint, Method.Post)
            .AddParameter("value", segmentationId)
            .AddParameter("type", "DynamicContent");
        return await Client.ExecuteWithErrorHandlingFirst<IdDto>(request);
    }

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
