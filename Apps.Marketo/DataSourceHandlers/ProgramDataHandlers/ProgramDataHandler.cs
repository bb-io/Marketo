using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers.ProgramDataHandlers
{
    public class ProgramDataHandler : BaseInvocable, IAsyncDataSourceHandler
    {
        public ProgramDataHandler(InvocationContext invocationContext) : base(invocationContext)
        {
        }
        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/programs.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            request.AddQueryParameter("maxDepth", 10);
            var response = client.Paginate<ProgramDto>(request);
            return response.DistinctBy(x => x.Id).Where(str => str.Name.Contains(context.SearchString)).ToDictionary(k => k.Id.ToString(), v => v.Name);
        }
    }
}
