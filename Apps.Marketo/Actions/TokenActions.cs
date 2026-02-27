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

[ActionList("Tokens")]
public class TokenActions(InvocationContext invocationContext) : MarketoInvocable(invocationContext)
{
    [Action("Search tokens", Description = "Search all folder tokens")]
    public async Task<ListTokensResponse> ListTokens([ActionParameter] ListTokensRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{input.FolderId.Split("_").First()}/tokens.json"
            .SetQueryParameter("folderType", input.FolderId.Split("_").Last());
        var request = new RestRequest(endpoint, Method.Get);

        return await Client.ExecuteWithErrorHandlingFirst<ListTokensResponse>(request);
    }

    [Action("Get token", Description = "Get token by name")]
    public async Task<TokenDto> GetToken([ActionParameter] GetTokenRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{input.FolderId}/tokens.json"
            .SetQueryParameter("folderType", input.FolderType);
        var request = new RestRequest(endpoint, Method.Get);
        var tokens = await Client.ExecuteWithErrorHandlingFirst<ListTokensResponse>(request);

        var token = tokens.Tokens.FirstOrDefault(x => x.Name == input.TokenName);

        if (token == null && input.Recursive.HasValue && input.Recursive.Value)
        {
            var folderEndpoint = $"/rest/asset/v1/folder/{input.FolderId}.json".SetQueryParameter("type", input.FolderType);
            var folderRequest = new RestRequest(folderEndpoint, Method.Get);
            var response = await Client.ExecuteWithErrorHandlingFirst<FolderInfoDto>(folderRequest);
            if (response.Parent != null)
            {
                return await GetToken(new GetTokenRequest { FolderId = response.Parent.Value, FolderType = response.Parent.Type, Recursive = true, TokenName = input.TokenName });
            }
        }

        return token ?? new();
    }

    [Action("Create token", Description = "Create a new token")]
    public async Task<TokenDto> CreateToken([ActionParameter] CreateTokenRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{input.FolderId.Split("_").First()}/tokens.json";
        var request = new RestRequest(endpoint, Method.Post)
            .AddParameter("name", input.Name)
            .AddParameter("type", input.Type)
            .AddParameter("value", input.Value)
            .AddParameter("folderType", input.FolderId.Split("_").Last());

        var result = await Client.ExecuteWithErrorHandlingFirst<ListTokensResponse>(request);
        return result.Tokens.First();
    }

    [Action("Delete token", Description = "Delete specific token")]
    public async Task DeleteToken([ActionParameter] DeleteTokenRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{input.FolderId.Split("_").First()}/tokens/delete.json";
        var request = new RestRequest(endpoint, Method.Post)
            .AddParameter("name", input.Name)
            .AddParameter("type", input.Type)
            .AddParameter("folderType", input.FolderId.Split("_").Last());

        await Client.ExecuteWithErrorHandling(request);
    }
}