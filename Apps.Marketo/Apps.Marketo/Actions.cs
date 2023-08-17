using RestSharp;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common;
using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.Marketo.Models.Requests;
using Apps.Marketo.Models.Responses;

namespace Apps.Marketo
{
    [ActionList]
    public class Actions : BaseInvocable
    {
        public Actions(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        [Action("List all files", Description = "List all files")]
        public ListFilesResponse ListFiles()
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/files.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var response = client.Execute<FetchFileDto>(request);
            return new ListFilesResponse() { Files = response.Data.Result };
        }

        [Action("Get file info", Description = "Get file info")]
        public FileResponse GetFileInfo([ActionParameter] GetFileInfoRequest input)
        { 
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/file/{input.FileId}.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var response = client.Execute<FetchFileDto>(request);
            return response.Data.Result.First();
        }
    }
}