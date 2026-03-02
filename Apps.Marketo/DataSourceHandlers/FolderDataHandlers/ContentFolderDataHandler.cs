using RestSharp;
using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Identifiers.Optional;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.DataSourceHandlers.FolderDataHandlers;

public class ContentFolderDataHandler(
    InvocationContext invocationContext,
    [ActionParameter] OptionalContentTypesIdentifier contentTypesIdentifier)
    : MarketoInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        var request = new RestRequest($"/rest/asset/v1/folders.json", Method.Get);
        contentTypesIdentifier.ApplyDefaultValues();

        request.AddQueryParameter("maxDepth", 10);
        var response = await Client.Paginate<FolderInfoDto>(request);

        var allowedTypes = contentTypesIdentifier.ContentTypes!.ToList();
        if (allowedTypes.Contains(Constants.ContentTypes.Form))
            allowedTypes.Add("Landing Page Form");  // Because there's no 'Forms' in the folder type enum in the Marketo API

        var filteredFolders = response
            .DistinctBy(x => x.Id)
            .Where(str =>
                context.SearchString is null ||
                str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Where(x => allowedTypes.Contains(x.FolderType) || x.FolderId.Type == "Program");

        return filteredFolders
            .Select(x => new DataSourceItem($"{x.Id}_{x.FolderId.Type}", $"{x.Name} ({x.FolderId.Type})"))
            .ToList();
    }
}
