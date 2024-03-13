using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers
{
    public class SegmentationDataHandler : BaseInvocable, IAsyncDataSourceHandler
    {
        public SegmentationDataHandler(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/segmentation.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var response = client.Paginate<SegmenationDto>(request);
            return response.Where(str => str.Name.Contains(context.SearchString)).ToDictionary(k => k.Id.ToString(), v => v.Name);
        }
    }
}
