using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Entities.Email;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers;

public class EmailDataHandler : MarketoInvocable, IAsyncDataSourceHandler
{
    public EmailDataHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new RestRequest($"/rest/asset/v1/emails.json", Method.Get);
        var response = await Client.Paginate<EmailEntity>(request);
        return response.Where(str => context.SearchString is null || str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)).ToDictionary(k => k.Id.ToString(), v => v.Name);
    }
}