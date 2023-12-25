using RestSharp;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common;
using Apps.Marketo.Dtos;
using Apps.Marketo.Models;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.Marketo.Models.Files.Responses;
using Apps.Marketo.Models.Files.Requests;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Newtonsoft.Json;

namespace Apps.Marketo.Actions;

[ActionList]
public class FileActions : BaseActions
{
    private readonly IFileManagementClient _fileManagementClient;

    public FileActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
        : base(invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("List all files", Description = "List all files")]
    public ListFilesResponse ListFiles()
    {
        var request = new MarketoRequest("/rest/asset/v1/files.json", Method.Get, Credentials);
        var response = Client.ExecuteWithError<FileInfoDto>(request);
        return new ListFilesResponse() { Files = response.Result };
    }

    [Action("Get file info", Description = "Get file info")]
    public FileInfoDto GetFileInfo([ActionParameter] GetFileInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/file/{input.FileId}.json", Method.Get, Credentials);
        var response = Client.ExecuteWithError<FileInfoDto>(request);
        return response.Result.First();
    }

    [Action("Download file", Description = "Download file")]
    public FileWrapper DownloadFile([ActionParameter] GetFileInfoRequest input)
    {
        var fileInfo = GetFileInfo(input);
        var client = new RestClient();
        var request = new RestRequest(fileInfo.Url, Method.Get);
        var response = client.Get(request);

        using var stream = new MemoryStream(response.RawBytes);
        var file = _fileManagementClient.UploadAsync(stream, fileInfo.MimeType, fileInfo.Name).Result;
        return new FileWrapper { File = file };
    }
    
    [Action("Update file", Description = "Update file content")]
    public void UpdateFile([ActionParameter] UpdateFileRequest input)
    {
        var fileInfo = GetFileInfo(input);
        var request = new MarketoRequest($"/rest/asset/v1/file/{input.FileId}/content.json", Method.Post, Credentials);
        var fileBytes = _fileManagementClient.DownloadAsync(input.File).Result.GetByteData().Result;
        request.AddFile("file", fileBytes, input.File.Name, fileInfo.MimeType);
        request.AddParameter("id", input.FileId);
        Client.ExecuteWithError(request);
    }

    [Action("Upload file", Description = "Upload file")]
    public FileInfoDto UploadFile([ActionParameter] UploadFileRequest input)
    {
        var request = new MarketoRequest("/rest/asset/v1/files.json", Method.Post, Credentials);
        var fileBytes = _fileManagementClient.DownloadAsync(input.File).Result.GetByteData().Result;
        request.AddParameter("name", input.File.Name);
        request.AddParameter("description", input.Description);
        request.AddFile("file", fileBytes, input.File.Name);
        request.AddParameter("insertOnly", input.InsertOnly ?? true);
        request.AddParameter("folder", JsonConvert.SerializeObject(new {
            id = int.Parse(input.FolderId),
            type = input.Type
        }));
        var response = Client.ExecuteWithError<FileInfoDto>(request);
        return response.Result.First();
    }
}