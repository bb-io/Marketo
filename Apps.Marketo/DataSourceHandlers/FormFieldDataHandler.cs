using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Forms.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers
{
    public class FormFieldDataHandler : BaseInvocable, IDataSourceHandler
    {
        public GetFormRequest GetFormRequest { get; set; }

        public FormFieldDataHandler(InvocationContext invocationContext, [ActionParameter] GetFormRequest getFormRequest) : base(invocationContext)
        {
            GetFormRequest = getFormRequest;
        }

        public Dictionary<string, string> GetData(DataSourceContext context)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            if (GetFormRequest == null || string.IsNullOrWhiteSpace(GetFormRequest.FormId))
                throw new ArgumentException("Specify form first or set the output of \"List form fields\" action here");

            var formFields = GetFormFieldsFromSingleForm(client, GetFormRequest.FormId.ToString());

            return formFields.Where(formField => context.SearchString == null ||
                                                 (!string.IsNullOrEmpty(formField.Label) && formField.Label.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)))
                .ToDictionary(formField => formField.Id, formField => formField.Label ?? $"Empty label (id: {formField.Id})");
        }

        //private List<FormFieldDto> GetFormFieldsFromAllForms(MarketoClient client)
        //{
        //    var request = new MarketoRequest($"/rest/asset/v1/forms.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        //    var forms = client.Paginate<FormDto>(request);
        //    var allFormFields = new List<FormFieldDto>();
        //    foreach(var form in forms)
        //    {
        //        var getFieldsRequest = new MarketoRequest($"/rest/asset/v1/form/{form.Id}/fields.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        //        var formFields = GetFormFieldsFromSingleForm(client, form.Id.ToString());
        //        if (formFields != null)
        //        {
        //            foreach(var field in formFields)
        //            {
        //                field.Label = $"{field.Label ?? $"Empty label (id: {field.Id})"} - {form.Name}";
        //                allFormFields.Add(field);
        //            }
        //        }
        //    }
        //    return allFormFields;
        //}

        private List<FormFieldDto>? GetFormFieldsFromSingleForm(MarketoClient client, string formId)
        {
            var getFieldsRequest = new MarketoRequest($"/rest/asset/v1/form/{formId}/fields.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var formFields = client.ExecuteWithError<FormFieldDto>(getFieldsRequest);
            return formFields.Result;
        }
    }
}
