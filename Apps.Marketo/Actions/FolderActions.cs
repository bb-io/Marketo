﻿using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Folder.Requests;
using Apps.Marketo.Models.Folder.Responses;
using Apps.Marketo.Models.Program.Request;
using Apps.Marketo.Models.Tags.Request;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Newtonsoft.Json;
using RestSharp;
using System.Linq;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Marketo.Actions;

[ActionList]
public class FolderActions : MarketoInvocable
{
    public FolderActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [Action("Search folders", Description = "Search folders")]
    public ListFoldersResponse ListFolders([ActionParameter] ListFoldersRequest input)
    {
        var request = new MarketoRequest("/rest/asset/v1/folders.json", Method.Get, Credentials)
            .AddQueryParameter("maxDepth", input.MaxDepth ?? 10)
            .AddQueryParameter("workSpace", input.WorkSpace);
        if (!string.IsNullOrEmpty(input.RootManual))
            request.AddQueryParameter("root", input.RootManual);
        else if (!string.IsNullOrEmpty(input.Root))
            request.AddQueryParameter("root", input.Root);

        var response = Client.Paginate<FolderInfoDto>(request);

        if (input.Root != null)
            response = response.Where(x => x.Id != input.Root).ToList();

        if (input.FilterFolderTypes != null && input.FilterFolderTypes.Any())
            response = response.Where(x => input.FilterFolderTypes.Contains(x.FolderType)).ToList();

        if (input.UrlContainsFilter != null)
            response = response.Where(x => x.Path.Contains(input.UrlContainsFilter)).ToList();

        if (input.IncludeArchive == null || !input.IncludeArchive.Value)
            response = response.Where(x => !x.IsArchive).ToList();

        response.ForEach(x =>
        {
            x.SearchId = $"{x.Id}_{x.FolderId.Type}"; // TODO: This really shouldn't be necessary anymore. Check if that's true.
        });

        return new() { Folders = response };
    }

    [Action("Get folder info", Description = "Get folder info")]
    public FolderInfoDto GetFolderInfo([ActionParameter] GetFolderInfoRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{input.FolderId}.json".SetQueryParameter("type", input.FolderType);
        var request = new MarketoRequest(endpoint, Method.Get, Credentials);

        return Client.GetSingleEntity<FolderInfoDto>(request);
    }

    [Action("Get folder by name", Description = "Get folder by name")]
    public List<FolderInfoDto> GetFolderByName([ActionParameter][Display("Folder name")] string folderName)
    {
        var endpoint = $"/rest/asset/v1/folder/byName.json".SetQueryParameter("name", folderName);
        var request = new MarketoRequest(endpoint, Method.Get, Credentials);
        return Client.ExecuteWithError<FolderInfoDto>(request).Result!;
    }

    [Action("Create folder", Description = "Create folder")]
    public FolderInfoDto CreateFolder([ActionParameter] CreateFolderRequest input)
    {
        var request = new MarketoRequest("/rest/asset/v1/folders.json", Method.Post, Credentials)
            .AddParameter("description", input.Description)
            .AddParameter("name", input.Name)
            .AddParameter("parent", JsonConvert.SerializeObject(new
            {
                id = int.Parse(input.FolderId),
                type = input.Type ?? "Folder"
            }));

        return Client.GetSingleEntity<FolderInfoDto>(request);
    }

    [Action("Delete folder", Description = "Delete folder")]
    public void DeleteFolder([ActionParameter] GetFolderInfoRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{input.FolderId}/delete.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials);

        Client.ExecuteWithError<IdDto>(request);
    }

    [Action("Add tag to program", Description = "Add tag to program")]
    public void AddTagToFolder([ActionParameter] GetProgramRequest programRequest,
        [ActionParameter] GetTagTypeRequest tagTypeRequest,
        [ActionParameter] GetTagValueRequest tagValueRequest)
    {
        var endpoint = $"/rest/asset/v1/program/{programRequest.ProgramId}.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials);
        request.AddQueryParameter("tags", JsonConvert.SerializeObject(new[]
        {
            new
            {
                tagType = tagTypeRequest.TagType,
                tagValue = tagValueRequest.TagValue
            }
        }));
        Client.ExecuteWithError<IdDto>(request);
    }

    [Action("Get program tag value", Description = "Get program tag value")]
    public string GetProgramTagValue([ActionParameter] GetProgramRequest programRequest,
        [ActionParameter] GetTagTypeRequest tagTypeRequest,
        [ActionParameter] GetProgramTagRequest programTagRequest)
    {
        var endpoint = $"/rest/asset/v1/program/{programRequest.ProgramId}.json";
        var request = new MarketoRequest(endpoint, Method.Get, Credentials);
        var program = Client.GetSingleEntity<ProgramDto>(request);
        
        var tag = program.Tags.FirstOrDefault(x => x.TagType == tagTypeRequest.TagType);
        if (tag == null)
        {
            if (programTagRequest.IgnoreTagNotFoundError == true)
            {
                return string.Empty;
            }
            
            var tags = string.Join(", ", program.Tags.Select(x => $"[{x.TagType}] {x.TagValue}").ToList());
            throw new PluginMisconfigurationException($"We couldn't find the specified tag. All tags: {tags}");
        }
        
        return tag.TagValue;
    }
}