using Apps.Marketo.Dtos;
using Apps.Marketo.Models;
using Apps.Marketo.Models.Tokens.Request;
using Apps.Marketo.Models.Tokens.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using RestSharp;

namespace Apps.Marketo.Actions;

[ActionList]
public class TokenActions : BaseActions
{
    public TokenActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [Action("List tokens", Description = "List all folder tokens")]
    public ListTokensResponse ListTokens([ActionParameter] FolderRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{input.FolderId}/tokens.json"
            .SetQueryParameter("folderType", input.FolderType);
        var request = new MarketoRequest(endpoint, Method.Get, Credentials);
        
        return Client.ExecuteWithError<ListTokensResponse>(request).Result!.First();
    }
    
    [Action("Create token", Description = "Create a new token")]
    public TokenDto CreateToken(
        [ActionParameter] FolderRequest folder,
        [ActionParameter] CreateTokenRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{folder.FolderId}/tokens.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddParameter("name", input.Name)
            .AddParameter("type", input.Type)
            .AddParameter("value", input.Value)
            .AddParameter("folderType", folder.FolderType);
        
        return Client.ExecuteWithError<ListTokensResponse>(request).Result!.First().Tokens.First();
    }    
    
    [Action("Delete token", Description = "Delete specific token")]
    public void DeleteToken(
        [ActionParameter] FolderRequest folder,
        [ActionParameter] DeleteTokenRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{folder.FolderId}/tokens/delete.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddParameter("name", input.Name)
            .AddParameter("type", input.Type)
            .AddParameter("folderType", folder.FolderType);

        Client.ExecuteWithError(request);
    }
}