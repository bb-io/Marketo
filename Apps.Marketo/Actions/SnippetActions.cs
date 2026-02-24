using Apps.Marketo.Dtos;
using Apps.Marketo.HtmlHelpers;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models;
using Apps.Marketo.Models.Snippets.Request;
using Apps.Marketo.Models.Snippets.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Mime;
using System.Text;
using Apps.Marketo.Models.Entities;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Constants;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Apps.Marketo.Helper;

namespace Apps.Marketo.Actions;

[ActionList("Snippets")]
public class SnippetActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext)
{    
    [Action("Search snippets", Description = "Search snippets")]
    public async Task<ListSnippetsResponse> ListSnippets([ActionParameter] ListSnippetsRequest input)
    {
        var request = new RestRequest("/rest/asset/v1/snippets.json", Method.Get);
        if (input.Status != null) 
            request.AddQueryParameter("status", input.Status);
        var response = await Client.Paginate<SnippetDto>(request);

        if (input.EarliestUpdatedAt != null)
            response = response.Where(x => x.UpdatedAt >= input.EarliestUpdatedAt.Value).ToList();
        if (input.LatestUpdatedAt != null)
            response = response.Where(x => x.UpdatedAt <= input.LatestUpdatedAt.Value).ToList();

        response = input.NamePatterns != null ? 
            response.Where(x => FileFolderHelper.IsFilePathMatchingPattern(input.NamePatterns, x.Name, input.ExcludeMatched ?? false)).ToList() : 
            response;

        var result = string.IsNullOrEmpty(input.FolderId) ?
            response :
            response.Where(x => x.Folder.Value.ToString() == input.FolderId.Split("_").First()).ToList();
        return new(result);
    }

    [Action("Get snippet info", Description = "Get snippet info")]
    public async Task<SnippetDto> GetSnippetInfo([ActionParameter] SnippetIdentifier snippetRequest)
    {
        var endpoit = $"/rest/asset/v1/snippet/{snippetRequest.SnippetId}.json";
        var request = new RestRequest(endpoit, Method.Get);

        return await Client.ExecuteWithErrorHandlingFirst<SnippetDto>(request);
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

        return await Client.ExecuteWithErrorHandlingFirst<SnippetDto>(request);
    }

    [Action("Update snippet metadata", Description = "Update snippet metadata")]
    public async Task<SnippetDto> UpdateSnippetMetadata(
        [ActionParameter] SnippetIdentifier input,
        [ActionParameter] UpdateSnippetMetadataRequest updateSnippetMetadata)
    {
        var request = new RestRequest($"/rest/asset/v1/snippet/{input.SnippetId}.json", Method.Post);
        if (!string.IsNullOrEmpty(updateSnippetMetadata.Name))
            request.AddParameter("name", updateSnippetMetadata.Name);
        if (!string.IsNullOrEmpty(updateSnippetMetadata.Description))
            request.AddParameter("description", updateSnippetMetadata.Description);
        return await Client.ExecuteWithErrorHandlingFirst<SnippetDto>(request);
    }

    [Action("Get snippet as HTML for translation", Description = "Get snippet as HTML for translation")]
    public async Task<FileWrapper> GetSnippetAsHtml(
        [ActionParameter] SnippetIdentifier getSnippetRequest,
        [ActionParameter] SegmentationIdentifier getSegmentationRequest,
        [ActionParameter] SegmentIdentifier getSegmentBySegmentationRequest)
    {
        var snippetInfo = await GetSnippetInfo(getSnippetRequest);
        var snippetContentResponse = await GetSnippetContent(getSnippetRequest);

        if(snippetContentResponse.ContentItems.Count() == 1 && 
           snippetContentResponse.ContentItems.First().Type == "DynamicContent")
        {
            snippetContentResponse.ContentItems = 
                (await GetSnippetDynamicContent(getSnippetRequest, getSegmentationRequest, getSegmentBySegmentationRequest))
                .Select(x => new SnippetContentDto(x.Type, x.Content)).ToList();
        }
        var sectionContent = snippetContentResponse.ContentItems!
            .ToDictionary(
                x => x.Type,
                y => y.Content);
        var resultHtml = HtmlContentBuilder.GenerateHtml(
            sectionContent, 
            snippetInfo.Name, 
            getSegmentBySegmentationRequest.Segment, 
            new HtmlIdEntity(MetadataConstants.BlackbirdSnippetId, getSnippetRequest.SnippetId));

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        var file = await fileManagementClient.UploadAsync(stream, MediaTypeNames.Text.Html, $"{snippetInfo.Name}.html");
        return new() { File = file };
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
        
        var extractedSnippetId = HtmlContentBuilder.ExtractIdFromMeta(html, MetadataConstants.BlackbirdSnippetId);
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

    private async Task<List<SnippetSegmentDto>> GetSnippetDynamicContent(
        SnippetIdentifier getSnippetRequest,
        SegmentationIdentifier getSegmentationRequest,
        SegmentIdentifier getSegmentBySegmentationRequest)
    {
        var requestSeg = new RestRequest(
                $"/rest/asset/v1/snippet/{getSnippetRequest.SnippetId}/dynamicContent.json",
                Method.Get);
        var responseSeg = await Client.ExecuteWithErrorHandlingFirst<SnippetDynamicContentDto>(requestSeg);
        if (responseSeg.Segmentation.ToString() == getSegmentationRequest.SegmentationId)
            return responseSeg.Content
                .Where(x => x.SegmentName == getSegmentBySegmentationRequest.Segment)
                .ToList();
        throw new PluginMisconfigurationException("Segmentation does not match! " +
            "Looks like you choosed one segmentation, but your snippet already is segmented by another segmentation");
    }
}