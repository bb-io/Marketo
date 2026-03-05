using Apps.Marketo.Constants;
using Apps.Marketo.Dtos;
using Apps.Marketo.Dtos.Content;
using Apps.Marketo.Extensions;
using Apps.Marketo.Helper.FileFolder;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.Helper.Json;
using Apps.Marketo.HtmlHelpers;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Content.Response;
using Apps.Marketo.Models.Entities;
using Apps.Marketo.Models.Entities.LandingPage;
using Apps.Marketo.Services.Content.Models;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Mime;
using System.Text;

namespace Apps.Marketo.Services.Content.Concrete;

public class LandingPageContentService(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext), IContentService
{
    public async Task UploadContent(UploadContentInput input)
    {
        var metaTags = HtmlContentBuilder.ExtractAllMetaTags(input.HtmlContent);

        string pageId = HtmlContentBuilder.GetRequiredMetaValue(
            input.ContentId, metaTags, MetadataConstants.BlackbirdLandingPageId, "Landing page ID");

        string segmentationId = HtmlContentBuilder.GetRequiredMetaValue(
            input.SegmentationId, metaTags, MetadataConstants.BlackbirdSegmentationId, "Segmentation ID");

        string segment = HtmlContentBuilder.GetRequiredMetaValue(
            input.Segment, metaTags, MetadataConstants.BlackbirdSegmentName, "Segment name");

        var landingContentRequest = new RestRequest($"/rest/asset/v1/landingPage/{pageId}/content.json", Method.Get);
        var landingContentResponse = await Client.ExecuteWithErrorHandling<LandingPageContentDto>(landingContentRequest);

        if (input.UploadOnlyDynamicContent == false)
        {
            foreach (var item in landingContentResponse)
            {
                if (!JsonHelper.IsJsonObject(item.Content.ToString() ?? string.Empty) &&
                    (item.Type == "HTML" || item.Type == "RichText"))
                {
                    await ConvertSectionToDynamicContent(pageId, item.Id, segmentationId);
                }
            }
            landingContentResponse = await Client.ExecuteWithErrorHandling<LandingPageContentDto>(landingContentRequest);
        }

        var translatedContent = HtmlContentBuilder.ParseHtml(input.HtmlContent);
        var errors = new List<string>();
        foreach (var item in landingContentResponse)
        {
            if (JsonHelper.IsJsonObject(item.Content.ToString() ?? string.Empty) &&
                    (item.Type == "HTML" || item.Type == "RichText"))
            {
                var content = item.Content.ToString();
                var landingPageContent = JsonConvert.DeserializeObject<LandingPageContentValueDto>(content!)!;
                if (landingPageContent.ContentType == "DynamicContent" &&
                    !string.IsNullOrWhiteSpace(content) &&
                    translatedContent.TryGetValue(item.Id, out var translatedContentItem))
                {
                    await UpdateLandingDynamicContent(pageId, segment, landingPageContent.Content, item.Type, translatedContentItem);
                }
            }
        }
    }

    private async Task<IdDto> ConvertSectionToDynamicContent(string landingId, string htmlId, string segmentationId)
    {
        var endpoint = $"/rest/asset/v1/landingPage/{landingId}/content/{htmlId}.json";
        var request = new RestRequest(endpoint, Method.Post)
            .AddParameter("value", segmentationId)
            .AddParameter("type", "DynamicContent");
        return await Client.ExecuteWithErrorHandlingFirst<IdDto>(request);
    }

    private async Task UpdateLandingDynamicContent(
        string pageId,
        string segmentName,
        string dynamicContentId,
        string contentType,
        string content)
    {
        var endpoint =
            $"/rest/asset/v1/landingPage/{pageId}/dynamicContent/{dynamicContentId}.json";
        var request = new RestRequest(endpoint, Method.Post)
            .AddQueryParameter("segment", segmentName)
            .AddQueryParameter("type", contentType)
            .AddQueryParameter("value", content);

        await Client.ExecuteWithErrorHandlingFirst<IdDto>(request);
    }

    public async Task<FileReference> DownloadContent(DownloadContentRequest input)
    {
        var landingInfoRequest = new RestRequest($"/rest/asset/v1/landingPage/{input.ContentId}.json", Method.Get);
        var landingInfoResponse = await Client.ExecuteWithErrorHandlingFirst<LandingPageEntity>(landingInfoRequest);

        var landingContentRequest = new RestRequest($"/rest/asset/v1/landingPage/{input.ContentId}/content.json", Method.Get);
        var landingContentResponse = await Client.ExecuteWithErrorHandling<LandingPageContentDto>(landingContentRequest);

        if (landingContentResponse == null || !landingContentResponse.Any())
            throw new PluginMisconfigurationException($"No assets found for landing page ID {input.ContentId}");

        bool onlyDynamic = input.GetOnlyDynamicContent ?? false;
        bool includeImages = input.IncludeImages ?? false;

        var contentTasks = landingContentResponse
            .Where(x => (x.Type == "HTML" || x.Type == "RichText" || (includeImages && x.Type == "Image")) &&
                        (!onlyDynamic || JsonHelper.IsJsonObject(x.Content?.ToString() ?? string.Empty)))
            .Select(async item =>
            {
                var content = await GetLandingSectionContent(
                    landingInfoResponse.Id,
                    input.SegmentationId,
                    input.Segment,
                    item,
                    includeImages);

                return new KeyValuePair<string, string?>(item.Id, content);
            });

        var contentResults = await Task.WhenAll(contentTasks);

        var sectionContent = contentResults
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!);

        if (sectionContent.Count == 0)
            throw new PluginMisconfigurationException($"No matching content items found for landing page ID {input.ContentId} with the current filters.");

        var metadata = new List<MetadataEntity> { new(MetadataConstants.BlackbirdLandingPageId, input.ContentId) };

        bool isDynamicLandingPage = landingContentResponse.Any(x =>
            x.Content != null &&
            JsonHelper.IsJsonObject(x.Content.ToString() ?? string.Empty) &&
            x.Content.ToString()!.Contains("DynamicContent"));

        if (isDynamicLandingPage)
        {
            var resolvedSegmentName = string.IsNullOrWhiteSpace(input.Segment) ? "Default" : input.Segment;
            metadata.Add(new(MetadataConstants.BlackbirdSegmentName, resolvedSegmentName));

            if (!string.IsNullOrWhiteSpace(input.SegmentationId))
                metadata.Add(new(MetadataConstants.BlackbirdSegmentationId, input.SegmentationId));
        }

        var resultHtml = HtmlContentBuilder.GenerateHtml(
            sectionContent,
            landingInfoResponse.Name,
            metadata);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        return await fileManagementClient.UploadAsync(
            stream, 
            MediaTypeNames.Text.Html, 
            landingInfoResponse.Name.ToHtmlFileName());
    }

    public async Task<SearchContentResponse> SearchContent(SearchContentRequest input)
    {
        var request = new RestRequest($"/rest/asset/v1/landingPages.json", Method.Get);
        await FileFolderHelper.AddFolderParameter(Client, request, input.FolderId);
        request.AddQueryParameterIfNotNull("status", input.Status);

        var pages = await Client.Paginate<LandingPageEntity>(request);

        pages = pages
            .ApplyDateAfterFilter(input.CreatedAfter, x => x.CreatedAt)
            .ApplyDateBeforeFilter(input.CreatedBefore, x => x.CreatedAt)
            .ApplyDateAfterFilter(input.UpdatedAfter, x => x.UpdatedAt)
            .ApplyDateBeforeFilter(input.UpdatedBefore, x => x.UpdatedAt)
            .ApplyNamePatternFilter(input.NamePatterns, input.ExcludeMatched, x => x.Name);

        pages = await pages.ApplyIgnoreInArchiveFilter(Client, input.IgnoreInArchive, x => x.Folder);

        return new(pages.Select(x => new ContentDto(x)).ToList());
    }

    private async Task<string?> GetLandingSectionContent(
        string landingPageId,
        string? segmentationId,
        string? segment,
        LandingPageContentDto sectionContent,
        bool includeImages = false)
    {
        var domain = InvocationContext.AuthenticationCredentialsProviders.First(v => v.KeyName == "Munchkin Account ID").Value;
        if (JsonHelper.IsJsonObject(sectionContent.Content.ToString()) && JsonConvert.DeserializeObject<LandingPageContentValueDto>(sectionContent.Content.ToString()).ContentType == "DynamicContent")
        {
            var landingPageContent = JsonConvert.DeserializeObject<LandingPageContentValueDto>(sectionContent.Content.ToString());
            var requestSeg = new RestRequest(
                $"/rest/asset/v1/landingPage/{landingPageId}/dynamicContent/{landingPageContent.Content}.json",
                Method.Get);

            var responseSeg = await Client.ExecuteWithErrorHandling<LandingDynamicContentDto<LandingPageImageSegmentDto<object>>>(requestSeg);
            if (responseSeg.First().Segmentation.ToString() == segmentationId)
            {
                var imageSegment = responseSeg.First().Segments.Where(x => x.SegmentName == segment).FirstOrDefault();

                if (imageSegment != null && (imageSegment.Type == "Image") && includeImages)
                {
                    var imageDto = JsonConvert.DeserializeObject<LandingPageImageContent>(imageSegment.Content.ToString());
                    var imageUrl = string.IsNullOrWhiteSpace(imageDto.ContentUrl) ? imageDto.Content : imageDto.ContentUrl;

                    var builder = new UriBuilder(imageUrl);
                    builder.Host = $"{domain}.mktoweb.com";

                    string styleAttr = "";
                    if (imageSegment.FormattingOptions != null)
                        styleAttr = $" style=\"width:{imageSegment.FormattingOptions.Width};height:{imageSegment.FormattingOptions.Height};left:{imageSegment.FormattingOptions.Left ?? "0px"};top:{imageSegment.FormattingOptions.Top ?? "0px"};position:absolute\"";

                    return $"<img src=\"{builder.Uri}\"{styleAttr}>";
                }
                else if (imageSegment != null && imageSegment.Type == "Text")
                {
                    if (imageSegment.FormattingOptions != null)
                        return $"<div style=\"left:{imageSegment.FormattingOptions.Left ?? "0px"};top:{imageSegment.FormattingOptions.Top ?? "0px"};position:absolute\">{imageSegment.Content.ToString()}</div>";
                    else
                        return imageSegment.Content.ToString();
                }
                else
                {
                    var res = responseSeg.First().Segments
                    .Where(x => x.SegmentName == segment).First().Content.ToString();
                    if (imageSegment?.FormattingOptions != null)
                        return $"<div style=\"left:{imageSegment.FormattingOptions.Left ?? "0px"};top:{imageSegment.FormattingOptions.Top ?? "0px"};position:absolute\">{res}</div>";
                    else
                        return res;
                }
            }
            return string.Empty;
        }
        else if (sectionContent.Type == "Image")
        {
            var imageDto = JsonConvert.DeserializeObject<LandingPageImageContent>(sectionContent.Content.ToString());
            var imageUrl = string.IsNullOrWhiteSpace(imageDto.ContentUrl) ? imageDto.Content : imageDto.ContentUrl;

            var builder = new UriBuilder(imageUrl);
            builder.Host = $"{domain}.mktoweb.com";

            var styleAttr = "";
            if (sectionContent.FormattingOptions != null)
                styleAttr = $" style=\"width:{sectionContent.FormattingOptions.Width};height:{sectionContent.FormattingOptions.Height};left:{sectionContent.FormattingOptions.Left ?? "0px"};top:{sectionContent.FormattingOptions.Top ?? "0px"};position:absolute\"";

            return $"<img src=\"{builder.Uri}\"{styleAttr}>";
        }
        return sectionContent.Content.ToString();
    }
}
