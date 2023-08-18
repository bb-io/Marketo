using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Files.Requests;
using Apps.Marketo.Models.Files.Responses;
using Apps.Marketo.Models.Folder.Requests;
using Apps.Marketo.Models.Folder.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var response = client.Get<BaseResponseDto<FolderInfoDto>>(request);
            return new ListFoldersResponse() { Folders = response.Result};
        }

        [Action("Get folder info", Description = "Get folder info")]
        public FolderInfoDto GetFolderInfo([ActionParameter] GetFolderInfoRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/folder/{input.FolderId}.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var response = client.Execute<BaseResponseDto<FolderInfoDto>>(request);
            return response.Data.Result.First();
        }

        [Action("Create folder", Description = "Create folder")]
        public FolderInfoDto CreateFolder([ActionParameter] CreateFolderRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/folders.json", Method.Post, InvocationContext.AuthenticationCredentialsProviders);
            request.AddBody(new
            {
                description = input.Description,
                name = input.Name,
                parent = new
                {
                    id = input.FolderId,
                    type = input.Type
                }
            });
            var response = client.Execute<BaseResponseDto<FolderInfoDto>>(request);
            return response.Data.Result.First();
        }
    }
}
