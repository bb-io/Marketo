using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models;
using Apps.Marketo.Models.LandingPages.Requests;
using Apps.Marketo.Models.LandingPages.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json.Linq;
using Apps.Marketo.HtmlHelpers;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.Identifiers.Optional;
using Apps.Marketo.Constants;
using Apps.Marketo.Extensions;
using Apps.Marketo.Dtos.LandingPage;
using Apps.Marketo.Models.Entities.LandingPage;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.Helper.FileFolder;

namespace Apps.Marketo.Actions;

[ActionList("Landing pages")]
public class LandingPageActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext)
{
    [Action("Search landing pages", Description = "Search landing pages")]
    public async Task<SearchLandingPagesResponse> ListLandingPages([ActionParameter] SearchLandingPagesRequest input)
    {
        input.Validate();

        var request = new RestRequest($"/rest/asset/v1/landingPages.json", Method.Get);
        await FileFolderHelper.AddFolderParameter(Client, request, input.FolderId);
        request.AddQueryParameterIfNotNull("status", input.Status);

        var pages = await Client.Paginate<LandingPageEntity>(request);
        pages = pages.ApplyUpdatedAtFilter(input.UpdatedAfter, input.UpdatedBefore);
        pages = pages.ApplyCreatedAtFilter(input.CreatedAfter, input.CreatedBefore);
        pages = pages.ApplyNamePatternFilter(input.NamePatterns, input.ExcludeMatched);
        pages = await pages.ApplyIgnoreInArchiveFilter(Client, input.IgnoreInArchive);

        return new(pages.Select(x => new LandingPageDto(x)).ToList());
    }

    [Action("Get landing page info", Description = "Get landing page info")]
    public async Task<LandingPageDto> GetLandingInfo([ActionParameter] LandingPageIdentifier input)
    {
        var request = new RestRequest($"/rest/asset/v1/landingPage/{input.LandingPageId}.json", Method.Get);
        var result = await Client.ExecuteWithErrorHandlingFirst<LandingPageEntity>(request);
        return new(result);
    }

    [Action("Get landing page content", Description = "Get landing page content")]
    public async Task<LandingPageContentResponse> GetLandingContent([ActionParameter] LandingPageIdentifier input)
    {
        var request = new RestRequest($"/rest/asset/v1/landingPage/{input.LandingPageId}/content.json", Method.Get);
        var response = await Client.ExecuteWithErrorHandling<LandingPageContentDto>(request);
        return new(response.ToList());
    }

    [Action("Update landing page metadata", Description = "Update landing page metadata")]
    public async Task<LandingPageDto> UpdateLandingPageMetadata(
        [ActionParameter] LandingPageIdentifier input,
        [ActionParameter] UpdateLandingMetadataRequest updateLandingMetadata)
    {
        var request = new RestRequest($"/rest/asset/v1/landingPage/{input.LandingPageId}.json", Method.Post);
        request.AddParameterIfNotNull("name", updateLandingMetadata.Name);
        request.AddParameterIfNotNull("description", updateLandingMetadata.Description);

        var result = await Client.ExecuteWithErrorHandlingFirst<LandingPageEntity>(request);
        return new(result);
    }

    [Action("Create landing page", Description = "Create landing page")]
    public async Task<LandingPageDto> CreateLandingPage([ActionParameter] CreateLandingRequest input)
    {
        var request = new RestRequest($"/rest/asset/v1/landingPages.json", Method.Post);
        request.AddParameterIfNotNull("customHeadHTML", input.CustomHeadHTML);
        request.AddParameterIfNotNull("description", input.Description);
        request.AddParameterIfNotNull("facebookOgTags", input.FacebookOgTags);
        request.AddParameterIfNotNull("keywords", input.Keywords);
        request.AddParameter("mobileEnabled", input.MobileEnabled ?? false);
        request.AddParameter("prefillForm", input.PrefillForm ?? false);
        request.AddParameterIfNotNull("robots", input.Robots);
        request.AddParameter("template", int.Parse(input.Template));
        request.AddParameterIfNotNull("title", input.Title);
        request.AddParameterIfNotNull("urlPageName", input.UrlPageName);
        request.AddParameterIfNotNull("workspace", input.Workspace);
        request.AddParameter("folder", JsonConvert.SerializeObject(new
        {
            id = int.Parse(input.FolderId.Split("_").First()),
            type = input.FolderId.Split("_").Last()
        }));
        request.AddParameter("name", input.Name);

        var result = await Client.ExecuteWithErrorHandlingFirst<LandingPageEntity>(request);
        return new(result);
    }

    [Action("Delete landing page", Description = "Delete landing page")]
    public async Task DeleteLandingPage([ActionParameter] LandingPageIdentifier input)
    {
        var endpoint = $"/rest/asset/v1/landingPage/{input.LandingPageId}/delete.json";
        var request = new RestRequest(endpoint, Method.Post);

        await Client.ExecuteWithErrorHandling<IdDto>(request);
    }

    [Action("Approve landing page draft", Description = "Approve landing page draft")]
    public async Task ApproveLandingPage([ActionParameter] LandingPageIdentifier input)
    {
        var endpoint = $"/rest/asset/v1/landingPage/{input.LandingPageId}/approveDraft.json";
        var request = new RestRequest(endpoint, Method.Post);

        await Client.ExecuteWithErrorHandling<IdDto>(request);
    }

    [Action("Discard landing page draft", Description = "Discard landing page draft")]
    public async Task DiscardLandingPage([ActionParameter] LandingPageIdentifier input)
    {
        var endpoint = $"/rest/asset/v1/landingPage/{input.LandingPageId}/discardDraft.json";
        var request = new RestRequest(endpoint, Method.Post);

        await Client.ExecuteWithErrorHandling<IdDto>(request);
    }

    [Action("Unapprove landing page (back to draft)", Description = "Unapprove landing page (back to draft)")]
    public async Task UnapproveLandingPage([ActionParameter] LandingPageIdentifier input)
    {
        var endpoint = $"/rest/asset/v1/landingPage/{input.LandingPageId}/unapprove.json";
        var request = new RestRequest(endpoint, Method.Post);

        await Client.ExecuteWithErrorHandling<IdDto>(request);
    }

    [Action("Get landing page as HTML for translation", Description = "Get landing page as HTML for translation")]
    public async Task<FileWrapper> GetLandingPageAsHtml(
        [ActionParameter] LandingPageIdentifier getLandingPageInfoRequest,
        [ActionParameter] SegmentationIdentifier getSegmentationRequest,
        [ActionParameter] SegmentIdentifier getSegmentBySegmentationRequest,
        [ActionParameter] GetLandingPageAsHtmlRequest getLandingPageAsHtmlRequest)
    {
        var landingInfo = await GetLandingInfo(getLandingPageInfoRequest);
        var landingContentResponse = await GetLandingContent(getLandingPageInfoRequest);

        if (landingContentResponse.LandingPageContentItems == null
        || landingContentResponse.LandingPageContentItems.Count == 0)
        {
            throw new PluginMisconfigurationException($"No assets found for landing page ID {getLandingPageInfoRequest.LandingPageId}");
        }

        var onlyDynamic = getLandingPageAsHtmlRequest.GetOnlyDynamicContent.HasValue && getLandingPageAsHtmlRequest.GetOnlyDynamicContent.Value;
        var includeImages = getLandingPageAsHtmlRequest.IncludeImages.HasValue && getLandingPageAsHtmlRequest.IncludeImages.Value;

        var targetItems = landingContentResponse.LandingPageContentItems!
            .Where(x => (x.Type == "HTML" || x.Type == "RichText" || (includeImages && x.Type == "Image")) &&
                        ((onlyDynamic && IsJsonObject(x.Content?.ToString())) || !onlyDynamic))
            .ToList();

        if (targetItems.Count == 0)
        {
            throw new PluginMisconfigurationException($"No matching content items found for landing page ID {getLandingPageInfoRequest.LandingPageId}");
        }

        var contentTasks = targetItems.Select(async item =>
        {
            var content = await GetLandingSectionContent(
                getLandingPageInfoRequest,
                getSegmentationRequest,
                getSegmentBySegmentationRequest,
                item,
                includeImages);

            return new KeyValuePair<string, string>(item.Id, content);
        });

        var contentResults = await Task.WhenAll(contentTasks);
        var sectionContent = contentResults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var resultHtml = HtmlContentBuilder.GenerateHtml(
            sectionContent,
            landingInfo.Name,
            getSegmentBySegmentationRequest.Segment,
            new(MetadataConstants.BlackbirdLandingPageId, getLandingPageInfoRequest.LandingPageId));

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        var file = await fileManagementClient.UploadAsync(stream, MediaTypeNames.Text.Html, $"{landingInfo.Name}.html");
        return new() { File = file };
    }

    [Action("Translate landing page from HTML file", Description = "Translate landing page from HTML file")]
    public async Task<TranslateLandingWithHtmlResponse> TranslateLandingWithHtml(
        [ActionParameter] OptionalLandingPageIdentifier getLandingPageInfoRequest,
        [ActionParameter] SegmentationIdentifier getSegmentationRequest,
        [ActionParameter] SegmentIdentifier getSegmentBySegmentationRequest,
        [ActionParameter] TranslateLandingPageWithHtmlRequest translateLandingWithHtmlRequest)
    {
        var stream = await fileManagementClient.DownloadAsync(translateLandingWithHtmlRequest.File);
        var bytes = await stream.GetByteData();
        var html = Encoding.UTF8.GetString(bytes);
        
        var extractedId = HtmlContentBuilder.ExtractIdFromMeta(html, MetadataConstants.BlackbirdLandingPageId);
        
        var landingPageInfoRequest = new LandingPageIdentifier
        {
            LandingPageId = getLandingPageInfoRequest.LandingPageId ?? extractedId ?? throw new PluginMisconfigurationException("Landing page ID is not provided and not found in the HTML file. Please provide value in the optional input.")
        };
        
        var landingContentResponse = await GetLandingContent(landingPageInfoRequest);

        if (!translateLandingWithHtmlRequest.TranslateOnlyDynamic.HasValue ||
            !translateLandingWithHtmlRequest.TranslateOnlyDynamic.Value)
        {
            foreach (var item in landingContentResponse.LandingPageContentItems)
            {
                if (!IsJsonObject(item.Content.ToString()) &&
                    (item.Type == "HTML" || item.Type == "RichText"))
                {
                    await ConvertSectionToDynamicContent(landingPageInfoRequest.LandingPageId, item.Id, getSegmentationRequest.SegmentationId);
                }
            }
            landingContentResponse = await GetLandingContent(landingPageInfoRequest);
        }
        
        var translatedContent = HtmlContentBuilder.ParseHtml(html);
        var errors = new List<string>();
        foreach (var item in landingContentResponse.LandingPageContentItems)
        {
            if (IsJsonObject(item.Content.ToString()) &&
                    (item.Type == "HTML" || item.Type == "RichText"))
            {
                var content = item.Content.ToString();
                var landingPageContent = JsonConvert.DeserializeObject<LandingPageContentValueDto>(content!)!;
                if (landingPageContent.ContentType == "DynamicContent" && 
                    !string.IsNullOrWhiteSpace(content) && 
                    translatedContent.TryGetValue(item.Id, out var translatedContentItem))
                {
                    await UpdateLandingDynamicContent(landingPageInfoRequest, getSegmentBySegmentationRequest, landingPageContent.Content, item.Type, translatedContentItem);
                }
            }
        }
        return new()
        {
            Errors = errors
        };
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
        LandingPageIdentifier getLandingPageInfoRequest,
        SegmentIdentifier getSegmentBySegmentationRequest,
        string dynamicContentId,
        string contentType,
        string content)
    {
        var endpoint =
            $"/rest/asset/v1/landingPage/{getLandingPageInfoRequest.LandingPageId}/dynamicContent/{dynamicContentId}.json";
        var request = new RestRequest(endpoint, Method.Post)
            .AddQueryParameter("segment", getSegmentBySegmentationRequest.Segment)
            .AddQueryParameter("type", contentType)
            .AddQueryParameter("value", content);

        await Client.ExecuteWithErrorHandlingFirst<IdDto>(request);
    }

    private async Task<string?> GetLandingSectionContent(
        LandingPageIdentifier getLandingInfoRequest,
        SegmentationIdentifier getSegmentationRequest,
        SegmentIdentifier getSegmentBySegmentationRequest,
        LandingPageContentDto sectionContent,
        bool includeImages = false)
    {
        var domain = InvocationContext.AuthenticationCredentialsProviders.First(v => v.KeyName == "Munchkin Account ID").Value;
        if (IsJsonObject(sectionContent.Content.ToString()) && JsonConvert.DeserializeObject<LandingPageContentValueDto>(sectionContent.Content.ToString()).ContentType == "DynamicContent")
        {
            var landingPageContent = JsonConvert.DeserializeObject<LandingPageContentValueDto>(sectionContent.Content.ToString());
            var requestSeg = new RestRequest(
                    $"/rest/asset/v1/landingPage/{getLandingInfoRequest.LandingPageId}/dynamicContent/{landingPageContent.Content}.json",
                    Method.Get);

            var responseSeg = await Client.ExecuteWithErrorHandling<LandingDynamicContentDto<LandingPageImageSegmentDto<object>>>(requestSeg);
            if (responseSeg.First().Segmentation.ToString() == getSegmentationRequest.SegmentationId)
            {
                var imageSegment = responseSeg.First().Segments.Where(x => x.SegmentName == getSegmentBySegmentationRequest.Segment).FirstOrDefault();
                
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
                    .Where(x => x.SegmentName == getSegmentBySegmentationRequest.Segment).First().Content.ToString();
                    if (imageSegment?.FormattingOptions != null)
                        return $"<div style=\"left:{imageSegment.FormattingOptions.Left ?? "0px"};top:{imageSegment.FormattingOptions.Top ?? "0px"};position:absolute\">{res}</div>";
                    else
                        return res;
                }             
            }          
            return string.Empty;
        }
        else if (sectionContent.Type == "Image") // Static images
        {
            var imageDto = JsonConvert.DeserializeObject<LandingPageImageContent>(sectionContent.Content.ToString());
            var imageUrl = string.IsNullOrWhiteSpace(imageDto.ContentUrl) ? imageDto.Content : imageDto.ContentUrl;

            var builder = new UriBuilder(imageUrl);
            builder.Host = $"{domain}.mktoweb.com";

            var styleAttr = "";
            if(sectionContent.FormattingOptions != null)
                styleAttr = $" style=\"width:{sectionContent.FormattingOptions.Width};height:{sectionContent.FormattingOptions.Height};left:{sectionContent.FormattingOptions.Left ?? "0px"};top:{sectionContent.FormattingOptions.Top ?? "0px"};position:absolute\"";

            return $"<img src=\"{builder.Uri}\"{styleAttr}>";
        }
        return sectionContent.Content.ToString();
    }

    private bool IsJsonObject(string content)
    {
        bool isObject = false;
        try
        {
            JObject.Parse(content);
            isObject = true;
        }
        catch (Exception _) { }
        return isObject;
    }
}