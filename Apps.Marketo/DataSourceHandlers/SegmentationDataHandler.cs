using RestSharp;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Entities.Segmentation;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.DataSourceHandlers;

public class SegmentationDataHandler(InvocationContext invocationContext) 
    : MarketoInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        var request = new RestRequest($"/rest/asset/v1/segmentation.json", Method.Get);
        var response = await Client.Paginate<SegmenationEntity>(request);
        return response
            .Where(str => 
                context.SearchString is null || 
                str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(x => new DataSourceItem(x.Id, x.Name));
    }
}
