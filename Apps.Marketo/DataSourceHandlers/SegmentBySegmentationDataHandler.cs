using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Emails.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers
{
    public class SegmentBySegmentationDataHandler : BaseInvocable, IAsyncDataSourceHandler
    {
        public GetSegmentationRequest SegmentationRequest { get; set; }
        public SegmentBySegmentationDataHandler(InvocationContext invocationContext, [ActionParameter] GetSegmentationRequest segmentationRequest) : base(invocationContext)
        {
            SegmentationRequest = segmentationRequest;
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            if (SegmentationRequest == null)
                throw new ArgumentException("Please, specify segmentation first");
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/segmentation/{SegmentationRequest.SegmentationId}/segments.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var response = client.Paginate<SegmentDto>(request);
            return response.Where(str => str.Name.Contains(context.SearchString)).ToDictionary(k => k.Name, v => v.Name);
        }
    }
}
