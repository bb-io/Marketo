using Apps.Marketo.Api;
using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Marketo.Invocables;

public class MarketoInvocable : BaseInvocable
{
    protected readonly IEnumerable<AuthenticationCredentialsProvider> Credentials;
    protected readonly MarketoClient Client;

    protected MarketoInvocable(InvocationContext invocationContext) : base(invocationContext)
    {
        Credentials = invocationContext.AuthenticationCredentialsProviders;
        Client = new(Credentials);
    }

    protected async Task<IEnumerable<string>?> AddFolderParameter(RestRequest request, string? folderId, bool isRecursive = false)
    {
        if (folderId != null)
        {
            if (folderId.Contains("_Folder"))
            {
                var folderParsedId = int.Parse(folderId.Replace("_Folder", ""));
                request.AddQueryParameter("folder", folderParsedId);
                return isRecursive ? await ListFoldersInSpecifiedFolder(folderParsedId, "Folder") : null;
            }  
            else if (folderId.Contains("_Program"))
            {
                var programParsedId = int.Parse(folderId.Replace("_Program", ""));
                request.AddQueryParameter("folder", JsonConvert.SerializeObject(new
                {
                    id = programParsedId,
                    type = "Program"
                }));
                return isRecursive ? await ListFoldersInSpecifiedFolder(programParsedId, "Program") : null;
            }
        }
        return null;
    }

    protected static bool IsFilePathMatchingPattern(List<string> patterns, string filePath, bool exclude)
    {
        var matcher = new Matcher();
        if (exclude)
        {
            matcher.AddInclude("*");
            matcher.AddExcludePatterns(patterns);
        }
        else
        {
            matcher.AddIncludePatterns(patterns);
        }           
        return matcher.Match(filePath).HasMatches;
    }

    private async Task<IEnumerable<string>> ListFoldersInSpecifiedFolder(int folderId, string folderType)
    {
        var request = new RestRequest($"/rest/asset/v1/folder/{folderId}/content.json", Method.Get);
        request.AddQueryParameter("type", folderType);
        var response = await Client.Paginate<FolderTypeInfoDto>(request);
        return response.Where(x => x.Type == "Program" || x.Type == "Folder").Select(x => $"{x.Id}_{x.Type}").ToList();
    }

    protected async Task<bool> IsAssetInArchieveFolder(AssetFolder folderDto)
    {
        var endpoint = $"/rest/asset/v1/folder/{folderDto.Value}.json".SetQueryParameter("type", folderDto.Type);
        var request = new RestRequest(endpoint, Method.Get);
        var emailFolderInfo = await Client.ExecuteWithErrorHandlingFirst<FolderInfoDto>(request);
        await Task.Delay(500);
        return emailFolderInfo.IsArchive;
    }
}