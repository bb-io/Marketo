using RestSharp;
using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.DataSourceHandlers.FolderDataHandlers;

public class TokenFolderDataHandler(InvocationContext invocationContext)
    : MarketoInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        var request = new RestRequest($"/rest/asset/v1/folders.json", Method.Get);
        request.AddQueryParameter("maxDepth", 10);
        var response = await Client.Paginate<FolderInfoDto>(request);

        return response
            .DistinctBy(x => x.Id)
            .Where(x => x.FolderType == "Marketing Folder" || x.FolderType == "Marketing Program")
            .Where(str => context.SearchString is null || str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(x => new DataSourceItem($"{x.Id}_{x.FolderId.Type}", x.Name))
            .ToList();
    }
}
