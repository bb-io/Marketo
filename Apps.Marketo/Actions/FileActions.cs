﻿using System.Net.Mime;
using RestSharp;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common;
using Apps.Marketo.Dtos;
using Apps.Marketo.Models;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.Marketo.Models.Files.Responses;
using Apps.Marketo.Models.Files.Requests;
using Newtonsoft.Json;
using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Marketo.Actions;

[ActionList]
public class FileActions : BaseInvocable
{
    public FileActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [Action("List all files", Description = "List all files")]
    public ListFilesResponse ListFiles()
    {
        var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
        var request = new MarketoRequest($"/rest/asset/v1/files.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        var response = client.ExecuteWithError<FileInfoDto>(request);
        return new ListFilesResponse() { Files = response.Result };
    }

    [Action("Get file info", Description = "Get file info")]
    public FileInfoDto GetFileInfo([ActionParameter] GetFileInfoRequest input)
    {
        var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
        var request = new MarketoRequest($"/rest/asset/v1/file/{input.FileId}.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        var response = client.ExecuteWithError<FileInfoDto>(request);
        return response.Result.First();
    }

    [Action("Download file", Description = "Download file")]
    public FileWrapper DownloadFile([ActionParameter] GetFileInfoRequest input)
    {
        var fileInfo = GetFileInfo(input);
        var client = new RestClient();
        var request = new RestRequest(fileInfo.Url, Method.Get);
        var response = client.Get(request).RawBytes;
        return new FileWrapper()
        {
            File = new File(response)
            {
                Name = fileInfo.Name,
                ContentType =  MediaTypeNames.Application.Octet // Uncommenct when "text/plain issue" is fixed; fileInfo.MimeType
            }
        };
    }
    
    [Action("Update file", Description = "Update file content")]
    public void UpdateFile([ActionParameter] UpdateFileRequest input)
    {
        var fileInfo = GetFileInfo(input);
        var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
       
        var request = new MarketoRequest($"/rest/asset/v1/file/{input.FileId}/content.json", Method.Post, InvocationContext.AuthenticationCredentialsProviders);
        request.AddFile("file", input.File.Bytes, input.File.Name, fileInfo.MimeType);
        request.AddParameter("id", input.FileId);
        
        client.ExecuteWithError(request);
    }

    [Action("Upload file", Description = "Upload file")]
    public FileInfoDto UploadFile([ActionParameter] UploadFileRequest input)
    {
        var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
        var request = new MarketoRequest($"/rest/asset/v1/files.json", Method.Post, InvocationContext.AuthenticationCredentialsProviders);
        request.AddParameter("name", input.File.Name);
        request.AddParameter("description", input.Description);
        request.AddFile("file", input.File.Bytes, input.File.Name);
        request.AddParameter("insertOnly", input.InsertOnly ?? true);
        request.AddParameter("folder", JsonConvert.SerializeObject(new {
            id = int.Parse(input.FolderId),
            type = input.Type
        }));
        var response = client.ExecuteWithError<FileInfoDto>(request);
        return response.Result.First();
    }
}