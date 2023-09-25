﻿using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Folder.Requests;
using Apps.Marketo.Models.Folder.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Marketo.Actions
{
    [ActionList]
    public class FolderActions : BaseInvocable
    {
        public FolderActions(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        [Action("List folders", Description = "List folders")]
        public ListFoldersResponse ListFolders([ActionParameter] ListFoldersRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/folders.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            request.AddQueryParameter("root", input.Root);
            request.AddQueryParameter("maxReturn", input.MaxReturn ?? 200);
            request.AddQueryParameter("maxDepth", input.MaxDepth ?? 10);
            request.AddQueryParameter("offset", input.Offset ?? 0);
            request.AddQueryParameter("workSpace", input.WorkSpace);
            var response = client.ExecuteWithError<FolderInfoDto>(request);
            return new ListFoldersResponse() { Folders = response.Result};
        }

        [Action("Get folder info", Description = "Get folder info")]
        public FolderInfoDto GetFolderInfo([ActionParameter] GetFolderInfoRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/folder/{input.FolderId}.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var response = client.ExecuteWithError<FolderInfoDto>(request);
            return response.Result.First();
        }

        [Action("Create folder", Description = "Create folder")]
        public FolderInfoDto CreateFolder([ActionParameter] CreateFolderRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/folders.json", Method.Post, InvocationContext.AuthenticationCredentialsProviders);
            request.AddParameter("description", input.Description);
            request.AddParameter("name", input.Name);
            request.AddParameter("parent", JsonConvert.SerializeObject(new
            {
                id = int.Parse(input.FolderId),
                type = input.Type ?? "Folder"
            }));
            var response = client.ExecuteWithError<FolderInfoDto>(request);
            return response.Result.First();
        }

        [Action("Delete folder", Description = "Delete folder")]
        public void DeleteFolder([ActionParameter] GetFolderInfoRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/folder/{input.FolderId}/delete.json", Method.Post, InvocationContext.AuthenticationCredentialsProviders);
            client.ExecuteWithError<IdDto>(request);
        }
    }
}
