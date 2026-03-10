using RestSharp;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common;
using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.Marketo.Models.Files.Responses;
using Apps.Marketo.Models.Files.Requests;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Newtonsoft.Json;
using Apps.Marketo.Models.Identifiers;

namespace Apps.Marketo.Actions;

[ActionList("Files")]
public class FileActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) 
    : MarketoInvocable(invocationContext)
{
    [Action("Search files", Description = "Search files")]
    public async Task<ListFilesResponse> ListFiles()
    {
        var request = new RestRequest("/rest/asset/v1/files.json", Method.Get);
        var response = await Client.ExecuteWithErrorHandling<FileInfoDto>(request);

        return new() { Files = response };
    }

    [Action("Get file", Description = "Get information of a specific file")]
    public async Task<FileInfoDto> GetFileInfo([ActionParameter] FileIdentifier input)
    {
        var endpoint = $"/rest/asset/v1/file/{input.FileId}.json";
        var request = new RestRequest(endpoint, Method.Get);

        return await Client.ExecuteWithErrorHandlingFirst<FileInfoDto>(request);
    }

    [Action("Download file", Description = "Download a specific file")]
    public async Task<FileWrapper> DownloadFile([ActionParameter] FileIdentifier input)
    {
        var fileInfo = await GetFileInfo(input);
        return new()
        {
            File = new(new HttpRequestMessage(HttpMethod.Get, fileInfo.Url), fileInfo.Name, fileInfo.MimeType)
        };
    }

    [Action("Update file", Description = "Update content of a specific file")]
    public async Task UpdateFile(
        [ActionParameter] FileIdentifier fileId,
        [ActionParameter] UpdateFileRequest input)
    {
        var fileInfo = await GetFileInfo(fileId);
        var fileBytes = fileManagementClient.DownloadAsync(input.File).Result.GetByteData().Result;

        var request = new RestRequest($"/rest/asset/v1/file/{fileId.FileId}/content.json", Method.Post)
            .AddFile("file", fileBytes, input.File.Name, fileInfo.MimeType)
            .AddParameter("id", fileId.FileId);

        await Client.ExecuteWithErrorHandling(request);
    }

    [Action("Upload file", Description = "Upload a file")]
    public async Task<FileInfoDto> UploadFile([ActionParameter] UploadFileRequest input)
    {
        var fileBytes = fileManagementClient.DownloadAsync(input.File).Result.GetByteData().Result;

        var request = new RestRequest("/rest/asset/v1/files.json", Method.Post)
            .AddParameter("name", input.File.Name)
            .AddParameter("description", input.Description)
            .AddFile("file", fileBytes, input.File.Name)
            .AddParameter("insertOnly", input.InsertOnly ?? true)
            .AddParameter("folder", JsonConvert.SerializeObject(new
            {
                id = int.Parse(input.FolderId),
                type = input.Type
            }));
        
        return await Client.ExecuteWithErrorHandlingFirst<FileInfoDto>(request);
    }
}