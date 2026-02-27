using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Folder.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers.FolderDataHandlers;

public class FolderDataHandler(InvocationContext invocationContext, [ActionParameter] GetFolderInfoRequest folderRequest)
    : MarketoInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        var request = new RestRequest($"/rest/asset/v1/folders.json", Method.Get)
            .AddQueryParameter("maxDepth", 10);
        
        if(!string.IsNullOrEmpty(folderRequest.RootFolder))
            request.AddQueryParameter("root", folderRequest.RootFolder);
        
        var response = await Client.Paginate<FolderInfoDto>(request);
        return response
            .DistinctBy(x => x.Id)
            .Where(str =>
                context.SearchString is null ||
                str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(x => new DataSourceItem(x.Id, x.Name))
            .ToList();
    }
}