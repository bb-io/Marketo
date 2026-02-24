using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers.FolderDataHandlers
{
    public class TokenFolderDataHandler(InvocationContext invocationContext)
        : MarketoInvocable(invocationContext), IAsyncDataSourceHandler
    {
        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var request = new RestRequest($"/rest/asset/v1/folders.json", Method.Get);
            request.AddQueryParameter("maxDepth", 10);
            var response = await Client.Paginate<FolderInfoDto>(request);
            return response
                .DistinctBy(x => x.Id)
                .Where(x => x.FolderType == "Marketing Folder" || x.FolderType == "Marketing Program")
                .Where(str => context.SearchString is null || str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(k => $"{k.Id.ToString()}_{k.FolderId.Type}", v => v.Name);
        }
    }
}
