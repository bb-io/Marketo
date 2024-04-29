using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common;
using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers;

public class EmailDataHandler : BaseInvocable, IAsyncDataSourceHandler
{
    public EmailDataHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
        var request = new MarketoRequest($"/rest/asset/v1/emails.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        var response = client.Paginate<EmailDto>(request);
        return response.Where(str => context.SearchString is null || str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)).ToDictionary(k => k.Id.ToString(), v => v.Name);
    }
}