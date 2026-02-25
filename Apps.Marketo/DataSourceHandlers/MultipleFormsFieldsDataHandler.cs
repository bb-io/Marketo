using Apps.Marketo.Dtos;
using Apps.Marketo.Dtos.Form;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Forms.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers;

public class MultipleFormsFieldsDataHandler(
    InvocationContext invocationContext, 
    [ActionParameter] GetMultipleFormsRequest input) 
    : MarketoInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        if (input == null || input.Forms == null)
            throw new PluginMisconfigurationException("Please provide the 'Forms' input first");

        var formFields = await GetFormFieldsFromAllForms(input.Forms);
        var filtered = formFields
            .Where(formField => 
                context.SearchString == null ||
                (!string.IsNullOrEmpty(formField.Label) && 
                formField.Label.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)));

        return filtered.Select(x => new DataSourceItem(x.Id, x.Label ?? $"Empty label (id: {x.Id})")).ToList();
    }

    private async Task<List<FormFieldDto>> GetFormFieldsFromAllForms(List<string> formIds)
    {
        var request = new RestRequest($"/rest/asset/v1/forms.json", Method.Get);
        var forms = (await Client.Paginate<FormDto>(request)).Where(x => formIds.Contains(x.Id.ToString())).ToList();
        var allFormFields = new List<FormFieldDto>();
        foreach (var form in forms)
        {
            var getFieldsRequest = new RestRequest($"/rest/asset/v1/form/{form.Id}/fields.json", Method.Get);
            var formFields = await GetFormFieldsFromSingleForm(form.Id.ToString());
            if (formFields != null)
            {
                foreach (var field in formFields)
                {
                    field.Label = $"{field.Label ?? $"Empty label (id: {field.Id})"} - {form.Name}";
                    field.Id = $"{field.Id} {form.Id.ToString()}";
                    allFormFields.Add(field);
                }
            }
        }
        return allFormFields;
    }

    private async Task<List<FormFieldDto>?> GetFormFieldsFromSingleForm(string formId)
    {
        var getFieldsRequest = new RestRequest($"/rest/asset/v1/form/{formId}/fields.json", Method.Get);
        var formFields = await Client.ExecuteWithErrorHandling<FormFieldDto>(getFieldsRequest);
        return formFields;
    }
}
