using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers;

public class SnippetDataHandler : BaseInvocable, IAsyncDataSourceHandler
{
    public SnippetDataHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);

        var request = new MarketoRequest("/rest/asset/v1/snippets.json", Method.Get,
            InvocationContext.AuthenticationCredentialsProviders);
        var items = client.Paginate<SnippetDto>(request);

        return items
            .Where(str =>
                context.SearchString is null ||
                str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .ToDictionary(k => k.Id.ToString(), v => v.Name);
    }
}