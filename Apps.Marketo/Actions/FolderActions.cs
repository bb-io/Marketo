using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Folder.Requests;
using Apps.Marketo.Models.Folder.Responses;
using Apps.Marketo.Models.Tags.Request;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Newtonsoft.Json;
using RestSharp;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Apps.Marketo.Models.Identifiers;

namespace Apps.Marketo.Actions;

[ActionList("Folders")]
public class FolderActions(InvocationContext invocationContext) : MarketoInvocable(invocationContext)
{
    [Action("Search folders", Description = "Search folders using specific criteria")]
    public async Task<ListFoldersResponse> ListFolders([ActionParameter] ListFoldersRequest input)
    {
        var request = new RestRequest("/rest/asset/v1/folders.json", Method.Get)
            .AddQueryParameter("maxDepth", input.MaxDepth ?? 10)
            .AddQueryParameter("workSpace", input.WorkSpace);
        if (!string.IsNullOrEmpty(input.RootManual))
            request.AddQueryParameter("root", input.RootManual);
        else if (!string.IsNullOrEmpty(input.Root))
            request.AddQueryParameter("root", input.Root);

        var response = await Client.Paginate<FolderInfoDto>(request);

        if (input.Root != null)
            response = response.Where(x => x.Id != input.Root).ToList();

        if (input.FilterFolderTypes != null && input.FilterFolderTypes.Any())
            response = response.Where(x => input.FilterFolderTypes.Contains(x.FolderType)).ToList();

        if (input.UrlContainsFilter != null)
            response = response.Where(x => x.Path.Contains(input.UrlContainsFilter)).ToList();

        if (input.IncludeArchive == null || !input.IncludeArchive.Value)
            response = response.Where(x => !x.IsArchive).ToList();

        response.ToList().ForEach(x =>
        {
            x.SearchId = $"{x.Id}_{x.FolderId.Type}"; // TODO: This really shouldn't be necessary anymore. Check if that's true.
        });

        return new() { Folders = response };
    }

    [Action("Get folder", Description = "Get information of a specific folder")]
    public async Task<FolderInfoDto> GetFolderInfo([ActionParameter] GetFolderInfoRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{input.FolderId}.json".SetQueryParameter("type", input.FolderType);
        var request = new RestRequest(endpoint, Method.Get);

        return await Client.ExecuteWithErrorHandlingFirst<FolderInfoDto>(request);
    }

    [Action("Get folder by name", Description = "Get information of a specific folder by its name")]
    public async Task<List<FolderInfoDto>> GetFolderByName([ActionParameter][Display("Folder name")] string folderName)
    {
        var endpoint = $"/rest/asset/v1/folder/byName.json".SetQueryParameter("name", folderName);
        var request = new RestRequest(endpoint, Method.Get);
        var result = await Client.ExecuteWithErrorHandling<FolderInfoDto>(request);
        return result.ToList();
    }

    [Action("Create folder", Description = "Create a folder")]
    public async Task<FolderInfoDto> CreateFolder([ActionParameter] CreateFolderRequest input)
    {
        var request = new RestRequest("/rest/asset/v1/folders.json", Method.Post)
            .AddParameter("description", input.Description)
            .AddParameter("name", input.Name)
            .AddParameter("parent", JsonConvert.SerializeObject(new
            {
                id = int.Parse(input.FolderId),
                type = input.Type ?? "Folder"
            }));

        return await Client.ExecuteWithErrorHandlingFirst<FolderInfoDto>(request);
    }

    [Action("Delete folder", Description = "Delete a specific folder")]
    public async Task DeleteFolder([ActionParameter] GetFolderInfoRequest input)
    {
        var endpoint = $"/rest/asset/v1/folder/{input.FolderId}/delete.json";
        var request = new RestRequest(endpoint, Method.Post);

        await Client.ExecuteWithErrorHandling<IdDto>(request);
    }

    [Action("Add tag to program", Description = "Add tag to a specific program")]
    public async Task AddTagToFolder(
        [ActionParameter] ProgramIdentifier programRequest,
        [ActionParameter] TagTypeIdentifier tagTypeRequest,
        [ActionParameter] TagValueIdentifier tagValueRequest)
    {
        var endpoint = $"/rest/asset/v1/program/{programRequest.ProgramId}.json";
        var request = new RestRequest(endpoint, Method.Post);
        request.AddQueryParameter("tags", JsonConvert.SerializeObject(new[]
        {
            new
            {
                tagType = tagTypeRequest.TagType,
                tagValue = tagValueRequest.TagValue
            }
        }));
        await Client.ExecuteWithErrorHandling<IdDto>(request);
    }

    [Action("Get program tag", Description = "Get value of a specific program tag")]
    public async Task<string> GetProgramTagValue(
        [ActionParameter] ProgramIdentifier programRequest,
        [ActionParameter] TagTypeIdentifier tagTypeRequest,
        [ActionParameter] GetProgramTagRequest programTagRequest)
    {
        var endpoint = $"/rest/asset/v1/program/{programRequest.ProgramId}.json";
        var request = new RestRequest(endpoint, Method.Get);
        var program = await Client.ExecuteWithErrorHandlingFirst<ProgramDto>(request);
        
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