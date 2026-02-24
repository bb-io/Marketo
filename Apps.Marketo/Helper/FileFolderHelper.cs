using Apps.Marketo.Api;
using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Marketo.Helper;

public static class FileFolderHelper
{
    public static async Task<IEnumerable<string>?> AddFolderParameter(
        MarketoClient client,
        RestRequest request, 
        string? folderId, 
        bool isRecursive = false)
    {
        if (folderId != null)
        {
            if (folderId.Contains("_Folder"))
            {
                var folderParsedId = int.Parse(folderId.Replace("_Folder", ""));
                request.AddQueryParameter("folder", folderParsedId);
                return isRecursive ? await ListFoldersInSpecifiedFolder(client, folderParsedId, "Folder") : null;
            }
            else if (folderId.Contains("_Program"))
            {
                var programParsedId = int.Parse(folderId.Replace("_Program", ""));
                request.AddQueryParameter("folder", JsonConvert.SerializeObject(new
                {
                    id = programParsedId,
                    type = "Program"
                }));
                return isRecursive ? await ListFoldersInSpecifiedFolder(client, programParsedId, "Program") : null;
            }
        }
        return null;
    }

    public static bool IsFilePathMatchingPattern(List<string> patterns, string filePath, bool exclude)
    {
        var matcher = new Matcher();
        if (exclude)
        {
            matcher.AddInclude("*");
            matcher.AddExcludePatterns(patterns);
        }
        else
            matcher.AddIncludePatterns(patterns);

        return matcher.Match(filePath).HasMatches;
    }

    public static async Task<IEnumerable<string>> ListFoldersInSpecifiedFolder(
        MarketoClient client, 
        int folderId, 
        string folderType)
    {
        var request = new RestRequest($"/rest/asset/v1/folder/{folderId}/content.json", Method.Get);
        request.AddQueryParameter("type", folderType);

        var response = await client.Paginate<FolderTypeInfoDto>(request);
        return response.Where(x => x.Type == "Program" || x.Type == "Folder").Select(x => $"{x.Id}_{x.Type}").ToList();
    }

    public static async Task<bool> IsAssetInArchievedFolder(MarketoClient client, AssetFolder folderDto)
    {
        var endpoint = $"/rest/asset/v1/folder/{folderDto.Value}.json".SetQueryParameter("type", folderDto.Type);
        var request = new RestRequest(endpoint, Method.Get);

        var response = await client.ExecuteWithErrorHandlingFirst<FolderInfoDto>(request);
        return response.IsArchive;
    }
}
