using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers;

public class FormDataHandler(InvocationContext invocationContext) 
    : MarketoInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        var request = new RestRequest("/rest/asset/v1/forms.json", Method.Get);
        var response = await Client.Paginate<FormDto>(request);
        
        var filtered = response.Where(form => 
            context.SearchString == null ||
            form.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));

        return filtered.Select(x => new DataSourceItem(x.Id.ToString(), x.Name)).ToList();
    }
}