using Apps.Marketo.Constants;
using Apps.Marketo.Dtos;
using Apps.Marketo.Dtos.LandingPage;
using Apps.Marketo.Extensions;
using Apps.Marketo.Helper.FileFolder;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Entities.LandingPage;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.Identifiers.Optional;
using Apps.Marketo.Models.LandingPages.Requests;
using Apps.Marketo.Models.LandingPages.Responses;
using Apps.Marketo.Services.Content;
using Apps.Marketo.Services.Content.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Marketo.Actions;

[ActionList("Landing pages")]
public class LandingPageActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext)
{
    private readonly ContentServiceFactory _factory = new(invocationContext, fileManagementClient);

    [Action("Search landing pages", Description = "Search landing pages")]
    public async Task<SearchLandingPagesResponse> ListLandingPages([ActionParameter] SearchLandingPagesRequest input)
    {
        input.Validate();

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
        var request = new RestRequest($"/rest/asset/v1/landingPage/{input.LandingPageId}.json", Method.Post)
            .AddParameterIfNotNull("name", updateLandingMetadata.Name)
            .AddParameterIfNotNull("description", updateLandingMetadata.Description);

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

    [Action("Download landing page content", Description = "Download landing page content")]
    public async Task<DownloadLandingPageResponse> GetLandingPageAsHtml(
        [ActionParameter] LandingPageIdentifier landingPageId,
        [ActionParameter] DownloadLandingPageRequest downloadInput)
    {
        var service = _factory.GetContentService(ContentTypes.LandingPage);
        var input = new DownloadContentRequest
        {
            ContentId = landingPageId.LandingPageId,
            GetOnlyDynamicContent = downloadInput.GetOnlyDynamicContent,
            IncludeImages = downloadInput.IncludeImages,
            SegmentationId = downloadInput.SegmentationId,
            Segment = downloadInput.Segment,
        };

        var file = await service.DownloadContent(input);
        return new(file);
    }

    [Action("Upload landing page content", Description = "Upload landing page content")]
    public async Task TranslateLandingWithHtml(
        [ActionParameter] OptionalLandingPageIdentifier landingPageInput,
        [ActionParameter] UploadLandingPageRequest uploadInput)
    {
        string html = await ContentDownloader.DownloadHtmlContent(fileManagementClient, uploadInput.File);
        var input = new UploadContentInput(html, landingPageInput, uploadInput);

        var service = _factory.GetContentService(ContentTypes.LandingPage);
        await service.UploadContent(input);
    }
}