﻿using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Emails.Requests;
using Apps.Marketo.Models.Emails.Responses;
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

namespace Apps.Marketo.Actions;

[ActionList]
public class LandingPageActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext)
{
    private const string BlackbirdLandingPageId = "blackbird-landing-page-id";
    
    [Action("Search landing pages", Description = "Search landing pages")]
    public ListLandingPagesResponse ListLandingPages([ActionParameter] ListLandingPagesRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/landingPages.json", Method.Get, Credentials);
        AddFolderParameter(request, input.FolderId);

        if (input.Status != null) request.AddQueryParameter("status", input.Status);
        var response = Client.Paginate<LandingPageDto>(request);
        if (input.EarliestUpdatedAt != null)
            response = response.Where(x => x.UpdatedAt >= input.EarliestUpdatedAt.Value).ToList();
        if (input.LatestUpdatedAt != null)
            response = response.Where(x => x.UpdatedAt <= input.LatestUpdatedAt.Value).ToList();

        response = input.NamePatterns != null ? response.Where(x => IsFilePathMatchingPattern(input.NamePatterns, x.Name, input.ExcludeMatched ?? false)).ToList() : response;
        return new() { LandingPages = response };
    }

    [Action("Get landing page info", Description = "Get landing page info")]
    public LandingPageDto GetLandingInfo([ActionParameter] GetLandingInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}.json", Method.Get, Credentials);
        return Client.GetSingleEntity<LandingPageDto>(request);
    }

    [Action("Get landing page content", Description = "Get landing page content")]
    public LandingPageContentResponse GetLandingContent([ActionParameter] GetLandingInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}/content.json", Method.Get, Credentials);
        var response = Client.ExecuteWithError<LandingPageContentDto>(request);
        return new(response.Result);
    }

    [Action("Update landing page metadata", Description = "Update landing page metadata")]
    public LandingPageDto UpdateLandingPageMetadata(
        [ActionParameter] GetLandingInfoRequest input,
        [ActionParameter] UpdateLandingMetadataRequest updateLandingMetadata)
    {
        var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}.json", Method.Post, Credentials);
        if (!string.IsNullOrEmpty(updateLandingMetadata.Name))
            request.AddParameter("name", updateLandingMetadata.Name);
        if (!string.IsNullOrEmpty(updateLandingMetadata.Description))
            request.AddParameter("description", updateLandingMetadata.Description);
        return Client.GetSingleEntity<LandingPageDto>(request);
    }

    [Action("Create landing page", Description = "Create landing page")]
    public LandingPageDto CreateLandingPage([ActionParameter] CreateLandingRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/landingPages.json", Method.Post, Credentials);
        if (input.CustomHeadHTML != null) request.AddParameter("customHeadHTML", input.CustomHeadHTML);
        if (input.Description != null) request.AddParameter("description", input.Description);
        if (input.FacebookOgTags != null) request.AddParameter("facebookOgTags", input.FacebookOgTags);
        if (input.Keywords != null) request.AddParameter("keywords", input.Keywords);
        request.AddParameter("mobileEnabled", input.MobileEnabled ?? false);
        request.AddParameter("prefillForm", input.PrefillForm ?? false);
        if (input.Robots != null) request.AddParameter("robots", input.Robots);
        request.AddParameter("template", int.Parse(input.Template));
        if (input.Title != null) request.AddParameter("title", input.Title);
        if (input.UrlPageName != null) request.AddParameter("urlPageName", input.UrlPageName);
        if (input.Workspace != null) request.AddParameter("workspace", input.Workspace);
        request.AddParameter("folder", JsonConvert.SerializeObject(new
        {
            id = int.Parse(input.FolderId.Split("_").First()),
            type = input.FolderId.Split("_").Last()
        }));
        request.AddParameter("name", input.Name);
        return Client.GetSingleEntity<LandingPageDto>(request);
    }

    [Action("Delete landing page", Description = "Delete landing page")]
    public void DeleteLandingPage([ActionParameter] GetLandingInfoRequest input)
    {
        var endpoint = $"/rest/asset/v1/landingPage/{input.Id}/delete.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials);

        Client.ExecuteWithError<IdDto>(request);
    }

    [Action("Approve landing page draft", Description = "Approve landing page draft")]
    public void ApproveLandingPage([ActionParameter] GetLandingInfoRequest input)
    {
        var endpoint = $"/rest/asset/v1/landingPage/{input.Id}/approveDraft.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials);

        try { Client.ExecuteWithError<IdDto>(request); }
        catch (BusinessRuleViolationException _) { }
    }

    [Action("Discard landing page draft", Description = "Discard landing page draft")]
    public void DiscardLandingPage([ActionParameter] GetLandingInfoRequest input)
    {
        var endpoint = $"/rest/asset/v1/landingPage/{input.Id}/discardDraft.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials);

        try { Client.ExecuteWithError<IdDto>(request); }
        catch (BusinessRuleViolationException _) {}
    }

    [Action("Unapprove landing page (back to draft)", Description = "Unapprove landing page (back to draft)")]
    public void UnapproveLandingPage([ActionParameter] GetLandingInfoRequest input)
    {
        var endpoint = $"/rest/asset/v1/landingPage/{input.Id}/unapprove.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials);

        try{ Client.ExecuteWithError<IdDto>(request); }
        catch (BusinessRuleViolationException e) 
        {
            if (e.Message != $"{input.Id} Landing Page is not approved")
                throw e;
        }
    }

    [Action("Get landing page as HTML for translation", Description = "Get landing page as HTML for translation")]
    public async Task<FileWrapper> GetLandingPageAsHtml(
        [ActionParameter] GetLandingInfoRequest getLandingPageInfoRequest,
        [ActionParameter] GetSegmentationRequest getSegmentationRequest,
        [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        [ActionParameter] GetLandingPageAsHtmlRequest getLandingPageAsHtmlRequest)
    {
        var landingInfo = GetLandingInfo(getLandingPageInfoRequest);
        var landingContentResponse = GetLandingContent(getLandingPageInfoRequest);
        var onlyDynamic = getLandingPageAsHtmlRequest.GetOnlyDynamicContent.HasValue && getLandingPageAsHtmlRequest.GetOnlyDynamicContent.Value;

        var sectionContent = landingContentResponse.LandingPageContentItems!
            .Where(x => (x.Type == "HTML" || x.Type == "RichText") && 
            ((onlyDynamic && IsJsonObject(x.Content.ToString())) || !onlyDynamic))
            .ToDictionary(
                x => x.Id,
                y => GetLandingSectionContent(getLandingPageInfoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, y));
        var resultHtml = HtmlContentBuilder.GenerateHtml(sectionContent, landingInfo.Name, getSegmentBySegmentationRequest.Segment, new(BlackbirdLandingPageId, getLandingPageInfoRequest.Id));

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        var file = await fileManagementClient.UploadAsync(stream, MediaTypeNames.Text.Html, $"{landingInfo.Name}.html");
        return new() { File = file };
    }

    [Action("Translate landing page from HTML file", Description = "Translate landing page from HTML file")]
    public async Task<TranslateLandingWithHtmlResponse> TranslateLandingWithHtml(
        [ActionParameter] GetLandingInfoOptionalRequest getLandingPageInfoRequest,
        [ActionParameter] GetSegmentationRequest getSegmentationRequest,
        [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        [ActionParameter] TranslateEmailWithHtmlRequest translateLandingWithHtmlRequest)
    {
        var stream = await fileManagementClient.DownloadAsync(translateLandingWithHtmlRequest.File);
        var bytes = await stream.GetByteData();
        var html = Encoding.UTF8.GetString(bytes);
        
        var extractedId = HtmlContentBuilder.ExtractIdFromMeta(html, BlackbirdLandingPageId);
        
        var landingPageInfoRequest = new GetLandingInfoRequest
        {
            Id = getLandingPageInfoRequest.Id ?? extractedId ?? throw new Exception("Landing page ID is not provided and not found in the HTML file. Please provide value in the optional input.")
        };
        
        var landingContentResponse = GetLandingContent(landingPageInfoRequest);

        if (!translateLandingWithHtmlRequest.TranslateOnlyDynamic.HasValue ||
            !translateLandingWithHtmlRequest.TranslateOnlyDynamic.Value)
        {
            foreach (var item in landingContentResponse.LandingPageContentItems)
            {
                if (!IsJsonObject(item.Content.ToString()) &&
                    (item.Type == "HTML" || item.Type == "RichText"))
                {
                    ConvertSectionToDynamicContent(landingPageInfoRequest.Id, item.Id, getSegmentationRequest.SegmentationId);
                }
            }
            landingContentResponse = GetLandingContent(landingPageInfoRequest);
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
                    var result = UpdateLandingDynamicContent(landingPageInfoRequest, getSegmentBySegmentationRequest, landingPageContent.Content, item.Type, translatedContentItem);
                    if(!string.IsNullOrEmpty(result))
                        errors.Add(result);
                }
            }
        }
        return new()
        {
            Errors = errors
        };
    }

    private IdDto ConvertSectionToDynamicContent(string landingId, string htmlId, string segmentationId)
    {
        var endpoint = $"/rest/asset/v1/landingPage/{landingId}/content/{htmlId}.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddParameter("value", segmentationId)
            .AddParameter("type", "DynamicContent");
        return Client.ExecuteWithError<IdDto>(request).Result.FirstOrDefault();
    }

    private string UpdateLandingDynamicContent(
        GetLandingInfoRequest getLandingPageInfoRequest,
        GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        string dynamicContentId,
        string contentType,
        string content)
    {
        var endpoint =
            $"/rest/asset/v1/landingPage/{getLandingPageInfoRequest.Id}/dynamicContent/{dynamicContentId}.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddQueryParameter("segment", getSegmentBySegmentationRequest.Segment)
            .AddQueryParameter("type", contentType)
            .AddQueryParameter("value", content);
        try
        {
            Client.GetSingleEntity<IdDto>(request);
            return null;
        }
        catch (Exception ex) 
        {
            return $"{ex.Message}, ContentId: {dynamicContentId}, Content: {content}";
        }
    }

    private string GetLandingSectionContent(
        GetLandingInfoRequest getLandingInfoRequest,
        GetSegmentationRequest getSegmentationRequest,
        GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        LandingPageContentDto sectionContent)
    {
        if (IsJsonObject(sectionContent.Content.ToString()))
        {
            var landingPageContent = JsonConvert.DeserializeObject<LandingPageContentValueDto>(sectionContent.Content.ToString());
            var requestSeg = new MarketoRequest(
                    $"/rest/asset/v1/landingPage/{getLandingInfoRequest.Id}/dynamicContent/{landingPageContent.Content}.json",
                    Method.Get, Credentials);

            var responseSeg = Client.ExecuteWithError<LandingDynamicContentDto>(requestSeg);
            if (responseSeg.Result!.First().Segmentation.ToString() == getSegmentationRequest.SegmentationId)
                return responseSeg.Result!.First().Segments
                    .Where(x => x.SegmentName == getSegmentBySegmentationRequest.Segment)
                    .Select(x => new GetEmailDynamicContentResponse(x)
                    { DynamicContentId = landingPageContent.Content }).First().Content;
            return string.Empty;
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