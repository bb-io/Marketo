using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers.ProgramDataHandlers
{
    public class ProgramDataHandler : MarketoInvocable, IAsyncDataSourceHandler
    {
        public ProgramDataHandler(InvocationContext invocationContext) : base(invocationContext)
        {
        }
        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var request = new RestRequest($"/rest/asset/v1/programs.json", Method.Get);
            request.AddQueryParameter("maxDepth", 10);
            var response = await Client.Paginate<ProgramDto>(request);
            return response.DistinctBy(x => x.Id).Where(str => str.Name.Contains(context.SearchString)).ToDictionary(k => k.Id.ToString(), v => v.Name);
        }
    }
}
