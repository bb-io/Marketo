using RestSharp;
using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.DataSourceHandlers;

public class SegmentBySegmentationDataHandler(
    InvocationContext invocationContext, 
    [ActionParameter] SegmentationIdentifier segmentationRequest) 
    : MarketoInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(segmentationRequest.SegmentationId))
            throw new PluginMisconfigurationException("Please specify segmentation first");

        var request = new RestRequest(
            $"/rest/asset/v1/segmentation/{segmentationRequest.SegmentationId}/segments.json", 
            Method.Get);

        var response = await Client.Paginate<SegmentDto>(request);
        return response
            .Where(str => 
                context.SearchString is null || 
                str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(x => new DataSourceItem(x.Name, x.Name))
            .ToList();
    }
}
