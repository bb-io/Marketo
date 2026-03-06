using Apps.Marketo.Constants;
using Apps.Marketo.Dtos;
using Apps.Marketo.Dtos.Snippet;
using Apps.Marketo.Extensions;
using Apps.Marketo.Helper.FileFolder;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Entities.Snippet;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.Identifiers.Optional;
using Apps.Marketo.Models.Snippets.Request;
using Apps.Marketo.Models.Snippets.Response;
using Apps.Marketo.Services.Content;
using Apps.Marketo.Services.Content.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Marketo.Actions;

[ActionList("Snippets")]
public class SnippetActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext)
{
    private readonly ContentServiceFactory _factory = new(invocationContext, fileManagementClient);

    [Action("Search snippets", Description = "Search snippets using specific criteria")]
    public async Task<SearchSnippetsResponse> ListSnippets([ActionParameter] SearchSnippetsRequest input)
    {
        input.Validate();

        var request = new RestRequest("/rest/asset/v1/snippets.json", Method.Get);
        request.AddQueryParameterIfNotNull("status", input.Status);

        var snippets = await Client.Paginate<SnippetEntity>(request);

        snippets = snippets
            .ApplyDateAfterFilter(input.CreatedAfter, x => x.CreatedAt)
            .ApplyDateBeforeFilter(input.CreatedBefore, x => x.CreatedAt)
            .ApplyDateAfterFilter(input.UpdatedAfter, x => x.UpdatedAt)
            .ApplyDateBeforeFilter(input.UpdatedBefore, x => x.UpdatedAt)
            .ApplyNamePatternFilter(input.NamePatterns, input.ExcludeMatched, x => x.Name)
            .ApplyFolderIdFilter(input.FolderId, x => x.Folder);

        snippets = await snippets.ApplyIgnoreInArchiveFilter(Client, input.IgnoreInArchive, x => x.Folder);

        return new(snippets.Select(x => new SnippetDto(x)).ToList());
    }

    [Action("Get snippet", Description = "Get information of a specific snippet")]
    public async Task<SnippetDto> GetSnippetInfo([ActionParameter] SnippetIdentifier snippetRequest)
    {
        var endpoit = $"/rest/asset/v1/snippet/{snippetRequest.SnippetId}.json";
        var request = new RestRequest(endpoit, Method.Get);

        var result = await Client.ExecuteWithErrorHandlingFirst<SnippetEntity>(request);
        return new(result);
    }

    [Action("Get snippet content", Description = "Get content of a specific snippet")]
    public async Task<ListSnippetContentResponse> GetSnippetContent([ActionParameter] SnippetIdentifier snippetRequest)
    {
        var request = new RestRequest($"/rest/asset/v1/snippet/{snippetRequest.SnippetId}/content.json", Method.Get);
        var response = await Client.ExecuteWithErrorHandling<SnippetContentDto>(request);

        return new()
        {
            ContentItems = response
        };
    }

    [Action("Create snippet", Description = "Create a snippet")]
    public async Task<SnippetDto> CreateSnippet([ActionParameter] CreateSnippetRequest snippetRequest)
    {
        var request = new RestRequest("/rest/asset/v1/snippets.json", Method.Post)
            .AddParameter("name", snippetRequest.Name)
            .AddParameter("description", snippetRequest.Description)
            .AddParameter("folder", JsonConvert.SerializeObject(new
            {
                id = snippetRequest.FolderId.Split("_").First(),
                type = snippetRequest.FolderId.Split("_").Last()
            }));

        var result = await Client.ExecuteWithErrorHandlingFirst<SnippetEntity>(request);
        return new(result);
    }

    [Action("Update snippet metadata", Description = "Update metadata of a specific snippet")]
    public async Task<SnippetDto> UpdateSnippetMetadata(
        [ActionParameter] SnippetIdentifier input,
        [ActionParameter] UpdateSnippetMetadataRequest updateSnippetMetadata)
    {
        var request = new RestRequest($"/rest/asset/v1/snippet/{input.SnippetId}.json", Method.Post)
            .AddParameterIfNotNull("name", updateSnippetMetadata.Name)
            .AddParameterIfNotNull("description", updateSnippetMetadata.Description);

        var result = await Client.ExecuteWithErrorHandlingFirst<SnippetEntity>(request);
        return new(result);
    }

    [Action("Download snippet content", Description = "Download content of a specific snippet")]
    public async Task<DownloadSnippetResponse> GetSnippetAsHtml(
        [ActionParameter] SnippetIdentifier snippetInput,
        [ActionParameter] DownloadSnippetRequest downloadInput)
    {
        var service = _factory.GetContentService(ContentTypes.Snippet);
        var input = new DownloadContentRequest
        {
            ContentId = snippetInput.SnippetId,
            SegmentationId = downloadInput.SegmentationId,
            Segment = downloadInput.Segment,
        };

        var file = await service.DownloadContent(input);
        return new(file);
    }

    [Action("Upload snippet content", Description = "Upload content of a specific snippet")]
    public async Task TranslateSnippetWithHtml(
        [ActionParameter] OptionalSnippetIdentifier snippetInput,
        [ActionParameter] UploadSnippetRequest uploadInput)
    {
        string html = await ContentDownloader.DownloadHtmlContent(fileManagementClient, uploadInput.File);
        var input = new UploadContentInput(html, snippetInput, uploadInput);

        var service = _factory.GetContentService(ContentTypes.Snippet);
        await service.UploadContent(input);
    }
}