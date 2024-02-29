using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Snippets.Request;
using Apps.Marketo.Models.Snippets.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Marketo.Actions;

[ActionList]
public class SnippetActions : MarketoInvocable
{
    public SnippetActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [Action("Search snippets", Description = "Search snippets")]
    public ListSnippetsResponse ListSnippets()
    {
        var request = new MarketoRequest("/rest/asset/v1/snippets.json", Method.Get, Credentials);
        return new()
        {
            Snippets = Client.Paginate<SnippetDto>(request)
        };
    }

    [Action("Get snippet", Description = "Get details of a specific snippet")]
    public SnippetDto GetSnippet([ActionParameter] SnippetRequest snippetRequest)
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
                id = snippetRequest.FolderId,
                type = snippetRequest.FolderType
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
}