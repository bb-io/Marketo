using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Tokens.Request;
using Apps.Marketo.Models.Tokens.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using RestSharp;

namespace Apps.Marketo.Actions;

[ActionList]
public class TokenActions : MarketoInvocable
{
    public TokenActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [Action("List tokens", Description = "List all folder tokens")]
    public ListTokensResponse ListTokens([ActionParameter] ListTokensRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{input.FolderId.Split("_").First()}/tokens.json"
            .SetQueryParameter("folderType", input.FolderId.Split("_").Last());
        var request = new MarketoRequest(endpoint, Method.Get, Credentials);

        return Client.GetSingleEntity<ListTokensResponse>(request);
    }

    [Action("Get token", Description = "Get token by name")]
    public TokenDto GetToken([ActionParameter] GetTokenRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{input.FolderId}/tokens.json"
            .SetQueryParameter("folderType", input.FolderType);
        var request = new MarketoRequest(endpoint, Method.Get, Credentials);
        var tokens = Client.GetSingleEntity<ListTokensResponse>(request);

        var token = tokens.Tokens.FirstOrDefault(x => x.Name == input.TokenName);

        if (token == null && input.Recursive.HasValue && input.Recursive.Value)
        {
            var folderEndpoint = $"/rest/asset/v1/folder/{input.FolderId}.json".SetQueryParameter("type", input.FolderType);
            var folderRequest = new MarketoRequest(folderEndpoint, Method.Get, Credentials);
            var response = Client.GetSingleEntity<FolderInfoDto>(folderRequest);
            if (response.Parent != null)
            {
                return GetToken(new GetTokenRequest { FolderId = response.Parent.Id, FolderType = response.Parent.Type, Recursive = true, TokenName = input.TokenName });
            }
        }

        return token ?? new();
    }

    [Action("Create token", Description = "Create a new token")]
    public TokenDto CreateToken(
        [ActionParameter] ListTokensRequest folderRequest,
        [ActionParameter] CreateTokenRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{folderRequest.FolderId.Split("_").First()}/tokens.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddParameter("name", input.Name)
            .AddParameter("type", input.Type)
            .AddParameter("value", input.Value)
            .AddParameter("folderType", folderRequest.FolderId.Split("_").Last());

        return Client.GetSingleEntity<ListTokensResponse>(request).Tokens.First();
    }

    [Action("Delete token", Description = "Delete specific token")]
    public void DeleteToken(
        [ActionParameter] ListTokensRequest folderRequest,
        [ActionParameter] DeleteTokenRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{folderRequest.FolderId.Split("_").First()}/tokens/delete.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddParameter("name", input.Name)
            .AddParameter("type", input.Type)
            .AddParameter("folderType", folderRequest.FolderId.Split("_").Last());

        Client.ExecuteWithErrorHandling(request);
    }
}