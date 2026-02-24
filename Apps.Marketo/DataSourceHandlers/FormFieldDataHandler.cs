using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers;

public class FormFieldDataHandler(
    InvocationContext invocationContext, 
    [ActionParameter] FormIdentifier formIdentifier) 
    : MarketoInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        if (formIdentifier == null || string.IsNullOrWhiteSpace(formIdentifier.FormId))
            throw new PluginMisconfigurationException("Please provide the 'Form ID' input first");

        var formFields = await GetFormFieldsFromSingleForm(formIdentifier.FormId);
        var filtered = formFields?
            .Where(formField => 
                context.SearchString == null || 
                (!string.IsNullOrEmpty(formField.Label) && 
                formField.Label.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))) ?? [];

        return filtered.Select(x => new DataSourceItem(x.Id, x.Label ?? $"Empty label (id: {x.Id})")).ToList();
    }

    private async Task<List<FormFieldDto>?> GetFormFieldsFromSingleForm(string formId)
    {
        var getFieldsRequest = new RestRequest($"/rest/asset/v1/form/{formId}/fields.json", Method.Get);
        var formFields = await Client.ExecuteWithErrorHandling<FormFieldDto>(getFieldsRequest);
        return formFields;
    }
}
