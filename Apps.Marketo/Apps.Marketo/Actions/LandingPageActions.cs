using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Folder.Requests;
using Apps.Marketo.Models.Folder.Responses;
using Apps.Marketo.Models.LandingPages.Requests;
using Apps.Marketo.Models.LandingPages.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Actions
{
    [ActionList]
    public class LandingPageActions : BaseInvocable
    {
        public LandingPageActions(InvocationContext invocationContext) : base(invocationContext)
        {
            
        }

        [Action("List landing pages", Description = "List landing pages")]
        public ListLandingPagesResponse ListLandingPages([ActionParameter] ListLandingPagesRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/landingPages.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            if (input.Status != null) request.AddQueryParameter("status", input.Status);
            request.AddQueryParameter("maxReturn", input.MaxReturn ?? 200);
            request.AddQueryParameter("offset", input.Offset ?? 0);
            if (input.FolderId != null) request.AddQueryParameter("folder", JsonConvert.SerializeObject(new { id = int.Parse(input.FolderId), type = input.Type ?? "Folder" }));
            var response = client.ExecuteWithError<LandingPageDto>(request);
            return new ListLandingPagesResponse() { LandingPages = response.Result };
        }

        [Action("Get landing page info", Description = "Get landing page info")]
        public LandingPageDto GetLandingInfo([ActionParameter] GetLandingInfoRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var response = client.ExecuteWithError<LandingPageDto>(request);
            return response.Result.First();
        }

        [Action("Create landing page", Description = "Create landing page")]
        public LandingPageDto CreateLandingPage([ActionParameter] CreateLandingRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/landingPages.json", Method.Post, InvocationContext.AuthenticationCredentialsProviders);
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
            var response = client.ExecuteWithError<LandingPageDto>(request);
            return response.Result.First();
        }

        [Action("Delete landing page", Description = "Delete landing page")]
        public void DeleteLandingPage([ActionParameter] GetLandingInfoRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}/delete.json", Method.Post, InvocationContext.AuthenticationCredentialsProviders);
            client.ExecuteWithError<IdDto>(request);
        }

        [Action("Approve landing page draft", Description = "Approve landing page draft")]
        public void ApproveLandingPage([ActionParameter] GetLandingInfoRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}/approveDraft.json", Method.Post, InvocationContext.AuthenticationCredentialsProviders);
            client.ExecuteWithError<IdDto>(request);
        }

        [Action("Discard landing page draft", Description = "Discard landing page draft")]
        public void DiscardLandingPage([ActionParameter] GetLandingInfoRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}/discardDraft.json", Method.Post, InvocationContext.AuthenticationCredentialsProviders);
            client.ExecuteWithError<IdDto>(request);
        }

        [Action("Unapprove landing page (back to draft)", Description = "Unapprove landing page (back to draft)")]
        public void UnapproveLandingPage([ActionParameter] GetLandingInfoRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}/unapprove.json", Method.Post, InvocationContext.AuthenticationCredentialsProviders);
            client.ExecuteWithError<IdDto>(request);
        }

        [Action("Get landing page full content", Description = "Get landing page full content")]
        public LandingPageContentDto GetLandingPageContent([ActionParameter] GetLandingInfoRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/landingPage/{input.Id}/fullContent.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var response = client.ExecuteWithError<LandingPageContentDto>(request);
            return response.Result.First();
        }
    }
}
