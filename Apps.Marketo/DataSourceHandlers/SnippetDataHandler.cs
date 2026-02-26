using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Entities.Snippet;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers;

public class SnippetDataHandler : MarketoInvocable, IAsyncDataSourceHandler
{
    public SnippetDataHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new RestRequest("/rest/asset/v1/snippets.json", Method.Get);
        var items = await Client.Paginate<SnippetEntity>(request);

        return items
            .Where(str =>
                context.SearchString is null ||
                str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .ToDictionary(k => k.Id.ToString(), v => v.Name);
    }
}