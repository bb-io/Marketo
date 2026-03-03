using Apps.Marketo.Constants;
using Apps.Marketo.Dtos;
using Apps.Marketo.Dtos.Snippet;
using Apps.Marketo.Extensions;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.HtmlHelpers;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Entities.Snippet;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.Snippets.Request;
using Apps.Marketo.Models.Snippets.Response;
using Apps.Marketo.Services.Content;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using RestSharp;
using System.Text;

namespace Apps.Marketo.Actions;

[ActionList("Snippets")]
public class SnippetActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext)
{
    private readonly ContentServiceFactory _factory = new(invocationContext, fileManagementClient);

    [Action("Search snippets", Description = "Search snippets")]
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

    [Action("Get snippet info", Description = "Get snippet info")]
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

    [Action("Create snippet", Description = "Create a new snippet")]
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

    [Action("Update snippet metadata", Description = "Update snippet metadata")]
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

    [Action("Download snippet content", Description = "Download snippet content")]
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

    [Action("Translate snippet from HTML file", Description = "Translate snippet from HTML file")]
    public async Task TranslateSnippetWithHtml(
        [ActionParameter] SnippetOptionalRequest getSnippetRequest,
        [ActionParameter] SegmentationIdentifier getSegmentationRequest,
        [ActionParameter] SegmentIdentifier getSegmentBySegmentationRequest,
        [ActionParameter] TranslateSnippetWithHtmlRequest translateSnippetWithHtmlRequest)
    {
        var stream = await fileManagementClient.DownloadAsync(translateSnippetWithHtmlRequest.File);
        var bytes = await stream.GetByteData();
        var html = Encoding.UTF8.GetString(bytes);
        
        var extractedSnippetId = HtmlContentBuilder.ExtractMeta(html, MetadataConstants.BlackbirdSnippetId);
        var snippetRequest = new SnippetIdentifier
        {
            SnippetId = getSnippetRequest.SnippetId ?? extractedSnippetId ??
                throw new Exception(
                    "Snippet ID is not provided and not found in the HTML file. Please provide value in the optional input.")
        };
        
        var snippetContentResponse = await GetSnippetContent(snippetRequest);
        if (!(snippetContentResponse.ContentItems.Count() == 1 &&
           snippetContentResponse.ContentItems.First().Type == "DynamicContent"))
        {
            await ConvertSnippetToDynamicContent(snippetRequest.SnippetId, getSegmentationRequest.SegmentationId);
        }
        var snippetDynamicContent = await GetSnippetDynamicContent(snippetRequest, getSegmentationRequest, getSegmentBySegmentationRequest);
        
        var translatedContent = HtmlContentBuilder.ParseHtml(html);
        foreach (var item in snippetDynamicContent.Where(x => x.SegmentName == getSegmentBySegmentationRequest.Segment).ToList())
        {
            if ((item.Type == "HTML" || item.Type == "Text") &&
                translatedContent.TryGetValue(item.Type, out var translatedContentItem))
            {
                await UpdateSnippetDynamicContent(snippetRequest, item.SegmentId.ToString(), item.Type, translatedContentItem);
            }
        }
    }

    private async Task<IdDto> ConvertSnippetToDynamicContent(string snippetId, string segmentationId)
    {
        var endpoint = $"/rest/asset/v1/snippet/{snippetId}/content.json";
        var request = new RestRequest(endpoint, Method.Post)
            .AddParameter("content", segmentationId)
            .AddParameter("type", "DynamicContent");
        return await Client.ExecuteWithErrorHandlingFirst<IdDto>(request);
    }

    private async Task<IdDto> UpdateSnippetDynamicContent(
        SnippetIdentifier getSnippetRequest,
        string segmentId,
        string contentType,
        string content)
    {
        var endpoint =
            $"/rest/asset/v1/snippet/{getSnippetRequest.SnippetId}/dynamicContent/{segmentId}.json";
        var request = new RestRequest(endpoint, Method.Post)
            .AddQueryParameter("type", contentType)
            .AddQueryParameter("value", content);

        return await Client.ExecuteWithErrorHandlingFirst<IdDto>(request);
    }

    private async Task<List<SnippetSegmentEntity>> GetSnippetDynamicContent(
        SnippetIdentifier getSnippetRequest,
        SegmentationIdentifier getSegmentationRequest,
        SegmentIdentifier getSegmentBySegmentationRequest)
    {
        var requestSeg = new RestRequest(
                $"/rest/asset/v1/snippet/{getSnippetRequest.SnippetId}/dynamicContent.json",
                Method.Get);
        var responseSeg = await Client.ExecuteWithErrorHandlingFirst<SnippetDynamicContentEntity>(requestSeg);
        if (responseSeg.Segmentation.ToString() == getSegmentationRequest.SegmentationId)
            return responseSeg.Content
                .Where(x => x.SegmentName == getSegmentBySegmentationRequest.Segment)
                .ToList();
        throw new PluginMisconfigurationException("Segmentation does not match! " +
            "Looks like you choosed one segmentation, but your snippet already is segmented by another segmentation");
    }
}