using RestSharp;
using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.DataSourceHandlers.FolderDataHandlers;

public class EmailTemplateFolderDataHandler(InvocationContext invocationContext) 
    : MarketoInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    private readonly string MarketoTemplatesFolderPath = "/Design Studio/Default/Emails/Marketo Templates";

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        var request = new RestRequest($"/rest/asset/v1/folders.json", Method.Get);
        request.AddQueryParameter("maxDepth", 10);

        var response = await Client.Paginate<FolderInfoDto>(request);
        return response
            .DistinctBy(x => x.Id)
            .Where(x => x.FolderType == "Email Template" && !x.Path.Contains(MarketoTemplatesFolderPath))
            .Where(str => context.SearchString is null || str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(x => new DataSourceItem($"{x.Id}_{x.FolderId.Type}", x.Name))
            .ToList();
    }
}
