using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Folder.Requests;
using Apps.Marketo.Models.Folder.Responses;
using Apps.Marketo.Models.Program.Request;
using Apps.Marketo.Models.Tags.Request;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Marketo.Actions;

[ActionList]
public class FolderActions : MarketoInvocable
{
    public FolderActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [Action("List folders", Description = "List folders")]
    public ListFoldersResponse ListFolders([ActionParameter] ListFoldersRequest input)
    {
        var request = new MarketoRequest("/rest/asset/v1/folders.json", Method.Get, Credentials)
            .AddQueryParameter("root", input.Root)
            .AddQueryParameter("maxDepth", input.MaxDepth ?? 10)
            .AddQueryParameter("workSpace", input.WorkSpace);

        var response = Client.Paginate<FolderInfoDto>(request);
        return new() { Folders = response };
    }

    [Action("Get folder info", Description = "Get folder info")]
    public FolderInfoDto GetFolderInfo([ActionParameter] GetFolderInfoRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{input.FolderId}.json";
        var request = new MarketoRequest(endpoint, Method.Get, Credentials);

        return Client.GetSingleEntity<FolderInfoDto>(request);
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

    [Action("Add tags to program", Description = "Add tags to program")]
    public void AddTagToFolder([ActionParameter] GetProgramRequest programRequest,
        [ActionParameter] GetTagTypeRequest tagTypeRequest,
        [ActionParameter] GetTagValueRequest tagValueRequest)
    {
        var endpoint = $"/rest/asset/v1/program/{programRequest.ProgramId}.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials);
        request.AddJsonBody(new
        {
            tags = tagValueRequest.TagValues.Select(x => new 
            { 
                tagType = tagTypeRequest.TagType,
                tagValue = x
            })
        });
        Client.ExecuteWithError<IdDto>(request);
    }
}