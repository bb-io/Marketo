using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers;

public class LandingPageHandler : MarketoInvocable, IAsyncDataSourceHandler
{
    public LandingPageHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/rest/asset/v1/landingPages.json", Method.Get);
        var response = await Client.Paginate<LandingPageDto>(request);
        return response.Where(str => context.SearchString is null || str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)).ToDictionary(k => k.Id.ToString(), v => v.Name);
    }
}