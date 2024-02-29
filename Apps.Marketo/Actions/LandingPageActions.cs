using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Emails.Requests;
using Apps.Marketo.Models.Emails.Responses;
using Apps.Marketo.Models;
using Apps.Marketo.Models.LandingPages.Requests;
using Apps.Marketo.Models.LandingPages.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json.Linq;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Newtonsoft.Json.Schema;

namespace Apps.Marketo.Actions;

[ActionList]
public class LandingPageActions : MarketoInvocable
{
    private const string HtmlIdAttribute = "id";

    private readonly IFileManagementClient _fileManagementClient;
    public LandingPageActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("List landing pages", Description = "List landing pages")]
    public ListLandingPagesResponse ListLandingPages([ActionParameter] ListLandingPagesRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/landingPages.json", Method.Get, Credentials);
        if (input.Status != null) request.AddQueryParameter("status", input.Status);
        if (input.FolderId != null)
            request.AddQueryParameter("folder",
                JsonConvert.SerializeObject(new { id = int.Parse(input.FolderId), type = input.Type ?? "Folder" }));

        var response = Client.Paginate<LandingPageDto>(request);
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
            id = int.Parse(input.FolderId),
            type = input.Type ?? "Folder"
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

        Client.ExecuteWithError<IdDto>(request);
    }

    [Action("Discard landing page draft", Description = "Discard landing page draft")]
    public void DiscardLandingPage([ActionParameter] GetLandingInfoRequest input)
    {
        var endpoint = $"/rest/asset/v1/landingPage/{input.Id}/discardDraft.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials);

        Client.ExecuteWithError<IdDto>(request);
    }

    [Action("Unapprove landing page (back to draft)", Description = "Unapprove landing page (back to draft)")]
    public void UnapproveLandingPage([ActionParameter] GetLandingInfoRequest input)
    {
        var endpoint = $"/rest/asset/v1/landingPage/{input.Id}/unapprove.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials);

        Client.ExecuteWithError<IdDto>(request);
    }

    [Action("Get landing page as HTML for translation", Description = "Get landing page as HTML for translation")]
    public async Task<FileWrapper> GetLandingPageAsHtml(
        [ActionParameter] GetLandingInfoRequest getLandingPageInfoRequest,
        [ActionParameter] GetSegmentationRequest getSegmentationRequest,
        [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest)
    {
        var landingInfo = GetLandingInfo(getLandingPageInfoRequest);
        var landingContentResponse = GetLandingContent(getLandingPageInfoRequest);

        var sectionContent = landingContentResponse.LandingPageContentItems!
            .Where(x => x.Type == "HTML" || x.Type == "RichText")
            .ToDictionary(
                x => x.Id,
                y => GetLandingSectionContent(getLandingPageInfoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, y));
        var resultHtml = GenerateHtml(sectionContent, landingInfo.Name, getSegmentBySegmentationRequest.Segment);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        var file = await _fileManagementClient.UploadAsync(stream, MediaTypeNames.Text.Html, $"{landingInfo.Name}.html");
        return new() { File = file };
    }

    [Action("Translate landing page from HTML file", Description = "Translate landing page from HTML file")]
    public void TranslateLandingWithHtml(
        [ActionParameter] GetLandingInfoRequest getLandingPageInfoRequest,
        [ActionParameter] GetSegmentationRequest getSegmentationRequest,
        [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        [ActionParameter] TranslateEmailWithHtmlRequest translateLandingWithHtmlRequest)
    {
        var landingContentResponse = GetLandingContent(getLandingPageInfoRequest);

        if (!translateLandingWithHtmlRequest.TranslateOnlyDynamic.HasValue ||
            !translateLandingWithHtmlRequest.TranslateOnlyDynamic.Value)
        {
            foreach (var item in landingContentResponse.LandingPageContentItems)
            {
                if (IsJsonObject(item.Content.ToString()))
                {
                    ConvertSectionToDynamicContent(getLandingPageInfoRequest.Id, item.Id, getSegmentationRequest.SegmentationId);
                }
            }
            landingContentResponse = GetLandingContent(getLandingPageInfoRequest);
        }
        var translatedContent = ParseHtml(translateLandingWithHtmlRequest.File);
        foreach (var item in landingContentResponse.LandingPageContentItems)
        {
            var landingPageContent = JsonConvert.DeserializeObject<LandingPageContentValueDto>(item.Content.ToString());
            if (landingPageContent.ContentType != null && landingPageContent.ContentType == "DynamicContent")
            {
                UpdateLandingDynamicContent(getLandingPageInfoRequest, getSegmentBySegmentationRequest, item.Content.ToString(), translatedContent[item.Id]);
            }
        }
    }

    private IdDto ConvertSectionToDynamicContent(string landingId, string htmlId, string segmentationId)
    {
        var endpoint = $"/rest/asset/v1/landingPage/{landingId}/content/{htmlId}.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddParameter("value", segmentationId)
            .AddParameter("type", "DynamicContent");
        return Client.ExecuteWithError<IdDto>(request).Result.FirstOrDefault();
    }

    private IdDto UpdateLandingDynamicContent(
        GetLandingInfoRequest getLandingPageInfoRequest,
        GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        string dynamicContentId,
        string content)
    {
        var endpoint =
            $"/rest/asset/v1/landingPage/{getLandingPageInfoRequest.Id}/dynamicContent/{dynamicContentId}.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddQueryParameter("segment", getSegmentBySegmentationRequest.Segment)
            .AddQueryParameter("type", "HTML")
            .AddQueryParameter("value", content);

        return Client.GetSingleEntity<IdDto>(request);
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
        }
        return sectionContent.Content.ToString();
    }

    private string GenerateHtml(Dictionary<string, string> sections,
        string title, string language)
    {
        var htmlDoc = new HtmlDocument();
        var htmlNode = htmlDoc.CreateElement("html");
        htmlDoc.DocumentNode.AppendChild(htmlNode);

        var headNode = htmlDoc.CreateElement("head");
        htmlNode.AppendChild(headNode);

        var titleNode = htmlDoc.CreateElement("title");
        headNode.AppendChild(titleNode);
        titleNode.InnerHtml = title;

        var bodyNode = htmlDoc.CreateElement("body");
        htmlNode.AppendChild(bodyNode);

        foreach (var section in sections)
        {
            if (!string.IsNullOrWhiteSpace(section.Value))
            {
                var sectionNode = htmlDoc.CreateElement("div");
                sectionNode.SetAttributeValue(HtmlIdAttribute, section.Key);
                sectionNode.InnerHtml = section.Value;
                bodyNode.AppendChild(sectionNode);
            }
        }
        return htmlDoc.DocumentNode.OuterHtml;
    }

    private Dictionary<string, string> ParseHtml(FileReference file)
    {
        var result = new Dictionary<string, string>();

        var formBytes = _fileManagementClient.DownloadAsync(file).Result.GetByteData().Result;
        var html = Encoding.UTF8.GetString(formBytes);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
        var sections = htmlDoc.DocumentNode.SelectSingleNode("//body").ChildNodes;
        foreach (var section in sections)
        {
            result.Add(section.Attributes[HtmlIdAttribute].Value, section.InnerHtml);
        }
        return result;
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