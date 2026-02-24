using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Folder.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers.FolderDataHandlers;

public class FolderDataHandler(InvocationContext invocationContext, [ActionParameter] GetFolderInfoRequest folderRequest)
    : MarketoInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/rest/asset/v1/folders.json", Method.Get);
        request.AddQueryParameter("maxDepth", 10);
        
        if(!string.IsNullOrEmpty(folderRequest.RootFolder))
        {
            request.AddQueryParameter("root", folderRequest.RootFolder);
        }
        
        var response = await Client.Paginate<FolderInfoDto>(request);
        return response.DistinctBy(x => x.Id).Where(str => context.SearchString is null || str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)).ToDictionary(k => k.Id.ToString(), v => v.Name);
    }
}