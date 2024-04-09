using Apps.Marketo.Dtos;
using Apps.Marketo.HtmlHelpers;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Emails.Requests;
using Apps.Marketo.Models.Emails.Responses;
using Apps.Marketo.Models.LandingPages.Requests;
using Apps.Marketo.Models;
using Apps.Marketo.Models.Snippets.Request;
using Apps.Marketo.Models.Snippets.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net.Mime;
using System.Text;
using System.Linq.Expressions;

namespace Apps.Marketo.Actions;

[ActionList]
public class SnippetActions : MarketoInvocable
{
    private readonly IFileManagementClient _fileManagementClient;

    public SnippetActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("Search snippets", Description = "Search snippets")]
    public ListSnippetsResponse ListSnippets([ActionParameter] ListSnippetsRequest input)
    {
        var request = new MarketoRequest("/rest/asset/v1/snippets.json", Method.Get, Credentials);
        if (input.Status != null) request.AddQueryParameter("status", input.Status);
        var response = Client.Paginate<SnippetDto>(request);

        if (input.EarliestUpdatedAt != null)
            response = response.Where(x => x.UpdatedAt >= input.EarliestUpdatedAt.Value).ToList();
        if (input.LatestUpdatedAt != null)
            response = response.Where(x => x.UpdatedAt <= input.LatestUpdatedAt.Value).ToList();
        return new()
        {
            Snippets = string.IsNullOrEmpty(input.FolderId) ? 
                        response : 
                        response.Where(x => x.Folder.Value.ToString() == input.FolderId.Split("_").First()).ToList()
        };
    }

    [Action("Get snippet info", Description = "Get snippet info")]
    public SnippetDto GetSnippetInfo([ActionParameter] SnippetRequest snippetRequest)
    {
        var endpoit = $"/rest/asset/v1/snippet/{snippetRequest.SnippetId}.json";
        var request = new MarketoRequest(endpoit, Method.Get, Credentials);

        return Client.GetSingleEntity<SnippetDto>(request);
    }

    [Action("Get snippet content", Description = "Get content of a specific snippet")]
    public ListSnippetContentResponse GetSnippetContent([ActionParameter] SnippetRequest snippetRequest)
    {
        var request = new MarketoRequest($"/rest/asset/v1/snippet/{snippetRequest.SnippetId}/content.json", Method.Get,
            Credentials);
        var response = Client.ExecuteWithError<SnippetContentDto>(request);

        return new()
        {
            ContentItems = response.Result!
        };
    }

    [Action("Create snippet", Description = "Create a new snippet")]
    public SnippetDto CreateSnippet([ActionParameter] CreateSnippetRequest snippetRequest)
    {
        var request = new MarketoRequest("/rest/asset/v1/snippets.json", Method.Post, Credentials)
            .AddParameter("name", snippetRequest.Name)
            .AddParameter("description", snippetRequest.Description)
            .AddParameter("folder", JsonConvert.SerializeObject(new
            {
                id = snippetRequest.FolderId.Split("_").First(),
                type = snippetRequest.FolderId.Split("_").Last()
            }));

        return Client.GetSingleEntity<SnippetDto>(request);
    }

    [Action("Update snippet content", Description = "Update content of a specific snippet")]
    public void UpdateSnippetContent(
        [ActionParameter] SnippetRequest snippetRequest,
        [ActionParameter] UpdateContentRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/snippet/{snippetRequest.SnippetId}/content.json", Method.Post,
                Credentials)
            .AddParameter("type", input.Type)
            .AddParameter("content", input.Content);

        Client.ExecuteWithErrorHandling(request);
    }

    [Action("Get snippet as HTML for translation", Description = "Get snippet as HTML for translation")]
    public async Task<FileWrapper> GetSnippetAsHtml(
        [ActionParameter] SnippetRequest getSnippetRequest,
        [ActionParameter] GetSegmentationRequest getSegmentationRequest,
        [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest)
    {
        var snippetInfo = GetSnippetInfo(getSnippetRequest);
        var snippetContentResponse = GetSnippetContent(getSnippetRequest);

        if(snippetContentResponse.ContentItems.Count() == 1 && 
           snippetContentResponse.ContentItems.First().Type == "DynamicContent")
        {
            snippetContentResponse.ContentItems = 
                GetSnippetDynamicContent(getSnippetRequest, getSegmentationRequest, getSegmentBySegmentationRequest).Select(x => new SnippetContentDto(x.Type, x.Content)).ToList();
        }
        var sectionContent = snippetContentResponse.ContentItems!
            .ToDictionary(
                x => x.Type,
                y => y.Content);
        var resultHtml = HtmlContentBuilder.GenerateHtml(sectionContent, snippetInfo.Name, getSegmentBySegmentationRequest.Segment);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        var file = await _fileManagementClient.UploadAsync(stream, MediaTypeNames.Text.Html, $"{snippetInfo.Name}.html");
        return new() { File = file };
    }

    [Action("Translate snippet from HTML file", Description = "Translate snippet from HTML file")]
    public void TranslateSnippetWithHtml(
        [ActionParameter] SnippetRequest getSnippetRequest,
        [ActionParameter] GetSegmentationRequest getSegmentationRequest,
        [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        [ActionParameter] TranslateSnippetWithHtmlRequest translateSnippetWithHtmlRequest)
    {
        var snippetContentResponse = GetSnippetContent(getSnippetRequest);
        if (!(snippetContentResponse.ContentItems.Count() == 1 &&
           snippetContentResponse.ContentItems.First().Type == "DynamicContent"))
        {
            ConvertSnippetToDynamicContent(getSnippetRequest.SnippetId, getSegmentationRequest.SegmentationId);
        }
        var snippetDynamicContent = GetSnippetDynamicContent(getSnippetRequest, getSegmentationRequest, getSegmentBySegmentationRequest);
        var translatedContent = HtmlContentBuilder.ParseHtml(translateSnippetWithHtmlRequest.File, _fileManagementClient);
        foreach (var item in snippetDynamicContent.Where(x => x.SegmentName == getSegmentBySegmentationRequest.Segment).ToList())
        {
            if ((item.Type == "HTML" || item.Type == "Text") &&
                translatedContent.TryGetValue(item.Type, out var translatedContentItem))
            {
                UpdateSnippetDynamicContent(getSnippetRequest, item.SegmentId.ToString(), item.Type, translatedContentItem);
            }
        }
    }

    private IdDto ConvertSnippetToDynamicContent(string snippetId, string segmentationId)
    {
        var endpoint = $"/rest/asset/v1/snippet/{snippetId}/content.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddParameter("content", segmentationId)
            .AddParameter("type", "DynamicContent");
        return Client.ExecuteWithError<IdDto>(request).Result.FirstOrDefault();
    }

    private IdDto UpdateSnippetDynamicContent(
        SnippetRequest getSnippetRequest,
        string segmentId,
        string contentType,
        string content)
    {
        var endpoint =
            $"/rest/asset/v1/snippet/{getSnippetRequest.SnippetId}/dynamicContent/{segmentId}.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddQueryParameter("type", contentType)
            .AddQueryParameter("value", content);
        try
        {
            return Client.GetSingleEntity<IdDto>(request);
        }
        catch (Exception ex) { }
        return default;
    }

    private List<SnippetSegmentDto> GetSnippetDynamicContent(
        SnippetRequest getSnippetRequest,
        GetSegmentationRequest getSegmentationRequest,
        GetSegmentBySegmentationRequest getSegmentBySegmentationRequest)
    {
        var requestSeg = new MarketoRequest(
                $"/rest/asset/v1/snippet/{getSnippetRequest.SnippetId}/dynamicContent.json",
                Method.Get, Credentials);
        var responseSeg = Client.ExecuteWithError<SnippetDynamicContentDto>(requestSeg);
        if (responseSeg.Result!.First().Segmentation.ToString() == getSegmentationRequest.SegmentationId)
            return responseSeg.Result!.First().Content
                .Where(x => x.SegmentName == getSegmentBySegmentationRequest.Segment)
                .ToList();
        throw new ArgumentException("Segmentation does not match! " +
            "Looks like you choosed one segmentation, but your snippet already is segmented by another segmentation");
    }
}