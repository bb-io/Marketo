using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Forms.Requests;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers
{
    public class MultipleFormsFieldsDataHandler : BaseInvocable, IDataSourceHandler
    {
        public GetMultipleFormsRequest GetMultipleFormsRequest { get; set; }

        public MultipleFormsFieldsDataHandler(InvocationContext invocationContext, [ActionParameter] GetMultipleFormsRequest getMultipleFormsRequest) : base(invocationContext)
        {
            GetMultipleFormsRequest = getMultipleFormsRequest;
        }

        public Dictionary<string, string> GetData(DataSourceContext context)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            if (GetMultipleFormsRequest == null || GetMultipleFormsRequest.Forms == null)
                throw new ArgumentException("Specify forms first");

            var formFields = GetFormFieldsFromAllForms(client, GetMultipleFormsRequest.Forms);

            return formFields.Where(formField => context.SearchString == null ||
                                                 (!string.IsNullOrEmpty(formField.Label) && formField.Label.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)))
                .ToDictionary(formField => formField.Id, formField => formField.Label ?? $"Empty label (id: {formField.Id})");
        }

        private List<FormFieldDto> GetFormFieldsFromAllForms(MarketoClient client, List<string> formIds)
        {
            var request = new MarketoRequest($"/rest/asset/v1/forms.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var forms = client.Paginate<FormDto>(request).Where(x => formIds.Contains(x.Id.ToString())).ToList();
            var allFormFields = new List<FormFieldDto>();
            foreach (var form in forms)
            {
                var getFieldsRequest = new MarketoRequest($"/rest/asset/v1/form/{form.Id}/fields.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
                var formFields = GetFormFieldsFromSingleForm(client, form.Id.ToString());
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

        private List<FormFieldDto>? GetFormFieldsFromSingleForm(MarketoClient client, string formId)
        {
            var getFieldsRequest = new MarketoRequest($"/rest/asset/v1/form/{formId}/fields.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var formFields = client.ExecuteWithError<FormFieldDto>(getFieldsRequest);
            return formFields.Result;
        }
    }
}
