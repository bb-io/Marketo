using System.Net.Mime;
using System.Text;
using Apps.Marketo.Dtos;
using Apps.Marketo.HtmlHelpers.Forms;
using Apps.Marketo.Models;
using Apps.Marketo.Models.LandingPages.Requests;
using Apps.Marketo.Models.LandingPages.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Marketo.Actions;

[ActionList]
public class LandingPageActions : BaseActions
{
    public LandingPageActions(InvocationContext invocationContext) : base(invocationContext) { }

    [Action("List landing pages", Description = "List landing pages")]
    public ListLandingPagesResponse ListLandingPages([ActionParameter] ListLandingPagesRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/landingPages.json", Method.Get, Credentials);
        if (input.Status != null) request.AddQueryParameter("status", input.Status);
        request.AddQueryParameter("maxReturn", input.MaxReturn ?? 200);
        request.AddQueryParameter("offset", input.Offset ?? 0);
        if (input.FolderId != null) request.AddQueryParameter("folder", JsonConvert.SerializeObject(new { id = int.Parse(input.FolderId), type = input.Type ?? "Folder" }));
        var response = Client.ExecuteWithError<LandingPageDto>(request);
        return new ListLandingPagesResponse() { LandingPages = response.Result };
    }

    [Action("Get landing page info", Description = "Get landing page info")]
    public LandingPageDto GetLandingInfo([ActionParameter] GetLandingInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}.json", Method.Get, Credentials);
        var response = Client.ExecuteWithError<LandingPageDto>(request);
        return response.Result.First();
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
        var response = Client.ExecuteWithError<LandingPageDto>(request);
        return response.Result.First();
    }

    [Action("Delete landing page", Description = "Delete landing page")]
    public void DeleteLandingPage([ActionParameter] GetLandingInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}/delete.json", Method.Post, Credentials);
        Client.ExecuteWithError<IdDto>(request);
    }

    [Action("Approve landing page draft", Description = "Approve landing page draft")]
    public void ApproveLandingPage([ActionParameter] GetLandingInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}/approveDraft.json", Method.Post, Credentials);
        Client.ExecuteWithError<IdDto>(request);
    }

    [Action("Discard landing page draft", Description = "Discard landing page draft")]
    public void DiscardLandingPage([ActionParameter] GetLandingInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}/discardDraft.json", Method.Post, Credentials);
        Client.ExecuteWithError<IdDto>(request);
    }

    [Action("Unapprove landing page (back to draft)", Description = "Unapprove landing page (back to draft)")]
    public void UnapproveLandingPage([ActionParameter] GetLandingInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}/unapprove.json", Method.Post, Credentials);
        Client.ExecuteWithError<IdDto>(request);
    }

    [Action("Get landing page full content", Description = "Get landing page full content")]
    public LandingPageContentDto GetLandingPageContent([ActionParameter] GetLandingInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}/fullContent.json", Method.Get, Credentials);
        var response = Client.ExecuteWithError<LandingPageContentDto>(request);
        return response.Result.First();
    }
}