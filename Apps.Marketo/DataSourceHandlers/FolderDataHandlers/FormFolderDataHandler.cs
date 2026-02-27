using Apps.Marketo.Constants;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.DataSourceHandlers.FolderDataHandlers;

public class FormFolderDataHandler(InvocationContext invocationContext) 
    : ContentFolderDataHandler(
        invocationContext,
        new() { ContentTypes = [ContentTypes.Form] }), IAsyncDataSourceItemHandler
{
    public new async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        return await base.GetDataAsync(context, ct);
    }
}
