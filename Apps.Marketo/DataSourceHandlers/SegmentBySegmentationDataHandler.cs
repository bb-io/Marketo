using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers
{
    public class SegmentBySegmentationDataHandler : MarketoInvocable, IAsyncDataSourceHandler
    {
        public SegmentationIdentifier SegmentationRequest { get; set; }
        public SegmentBySegmentationDataHandler(InvocationContext invocationContext, [ActionParameter] SegmentationIdentifier segmentationRequest) : base(invocationContext)
        {
            SegmentationRequest = segmentationRequest;
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            if (SegmentationRequest == null)
                throw new ArgumentException("Please, specify segmentation first");
            var request = new RestRequest($"/rest/asset/v1/segmentation/{SegmentationRequest.SegmentationId}/segments.json", Method.Get);
            var response = await Client.Paginate<SegmentDto>(request);
            return response.Where(str => context.SearchString is null || str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)).ToDictionary(k => k.Name, v => v.Name);
        }
    }
}
