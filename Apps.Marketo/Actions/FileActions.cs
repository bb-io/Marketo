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

namespace Apps.Marketo.Actions;

[ActionList]
public class FileActions : MarketoInvocable
{
    private readonly IFileManagementClient _fileManagementClient;

    public FileActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
        : base(invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("Search all files", Description = "Search all files")]
    public ListFilesResponse ListFiles()
    {
        var request = new MarketoRequest("/rest/asset/v1/files.json", Method.Get, Credentials);
        var response = Client.ExecuteWithError<FileInfoDto>(request);

        return new() { Files = response.Result };
    }

    [Action("Get file info", Description = "Get file info")]
    public FileInfoDto GetFileInfo([ActionParameter] GetFileInfoRequest input)
    {
        var endpoint = $"/rest/asset/v1/file/{input.FileId}.json";
        var request = new MarketoRequest(endpoint, Method.Get, Credentials);

        return Client.GetSingleEntity<FileInfoDto>(request);
    }

    [Action("Download file", Description = "Download file")]
    public FileWrapper DownloadFile([ActionParameter] GetFileInfoRequest input)
    {
        var fileInfo = GetFileInfo(input);
        return new()
        {
            File = new(new HttpRequestMessage(HttpMethod.Get, fileInfo.Url), fileInfo.Name, fileInfo.MimeType)
        };
    }

    [Action("Update file", Description = "Update file content")]
    public void UpdateFile([ActionParameter] UpdateFileRequest input)
    {
        var fileInfo = GetFileInfo(input);
        var fileBytes = _fileManagementClient.DownloadAsync(input.File).Result.GetByteData().Result;

        var request = new MarketoRequest($"/rest/asset/v1/file/{input.FileId}/content.json", Method.Post, Credentials)
            .AddFile("file", fileBytes, input.File.Name, fileInfo.MimeType)
            .AddParameter("id", input.FileId);

        Client.ExecuteWithErrorHandling(request);
    }

    [Action("Upload file", Description = "Upload file")]
    public FileInfoDto UploadFile([ActionParameter] UploadFileRequest input)
    {
        var fileBytes = _fileManagementClient.DownloadAsync(input.File).Result.GetByteData().Result;

        var request = new MarketoRequest("/rest/asset/v1/files.json", Method.Post, Credentials)
            .AddParameter("name", input.File.Name)
            .AddParameter("description", input.Description)
            .AddFile("file", fileBytes, input.File.Name)
            .AddParameter("insertOnly", input.InsertOnly ?? true)
            .AddParameter("folder", JsonConvert.SerializeObject(new
            {
                id = int.Parse(input.FolderId),
                type = input.Type
            }));
        
        return Client.GetSingleEntity<FileInfoDto>(request);
    }
}