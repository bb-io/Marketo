using RestSharp;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Entities.Form;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.DataSourceHandlers;

public class FormDataHandler(InvocationContext invocationContext) 
    : MarketoInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        var request = new RestRequest("/rest/asset/v1/forms.json", Method.Get);
        var response = await Client.Paginate<FormEntity>(request);
        
        var filtered = response.Where(form => 
            context.SearchString == null ||
            form.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));

        return filtered.Select(x => new DataSourceItem(x.Id.ToString(), x.Name)).ToList();
    }
}