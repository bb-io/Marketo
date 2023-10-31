using System.Net.Mime;
using System.Text;
using Apps.Marketo.Dtos;
using Apps.Marketo.HtmlHelpers.Forms;
using Apps.Marketo.Models.Files.Responses;
using Apps.Marketo.Models.Forms.Requests;
using Apps.Marketo.Models.Forms.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;
using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Marketo.Actions;

[ActionList]
public class FormActions : BaseInvocable
{
    private readonly IEnumerable<AuthenticationCredentialsProvider> _credentials;
    private readonly MarketoClient _client;

    public FormActions(InvocationContext invocationContext) : base(invocationContext)
    {
        _credentials = invocationContext.AuthenticationCredentialsProviders;
        _client = new MarketoClient(_credentials);
    }
    
    [Action("Get form", Description = "Get specified form.")]
    public FormDto GetForm([ActionParameter] GetFormRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/form/{input.FormId}.json", Method.Get, _credentials);
        var form = _client.ExecuteWithError<FormDto>(request).Result.First();
        return form;
    }

    [Action("List recently created or updated forms", Description = "List all forms that have been created or updated " +
                                                                    "during past hours. If number of hours is not specified, " +
                                                                    "forms created or updated during past 24 hours are listed.")]
    public ListFormsResponse ListRecentlyCreatedOrUpdatedForms([ActionParameter] ListFormsRequest input)
    {
        var startDateTime = (DateTime.Now - TimeSpan.FromHours(input.Hours ?? 24)).ToUniversalTime();
        var offset = 0;
        var maxReturn = 200;
        var forms = new List<FormDto>();
        BaseResponseDto<FormDto> response;

        do
        {
            var request = new MarketoRequest($"/rest/asset/v1/forms.json?maxReturn={maxReturn}&offset={offset}", 
                Method.Get, _credentials);
            response = _client.ExecuteWithError<FormDto>(request);
            var updatedForms = response.Result.Where(form => form.UpdatedAt >= startDateTime);
            
            if (input.Status != null)
                updatedForms = updatedForms.Where(form => form.Status == input.Status);
            
            forms.AddRange(updatedForms);
            offset += maxReturn;
        } while (response.Result.Count == maxReturn);

        return new ListFormsResponse { Forms = forms };
    }

    [Action("Get form as HTML for translation", Description = "Retrieve a form as HTML file for translation.")]
    public FileDataResponse GetFormAsHtml([ActionParameter] GetFormRequest input)
    {
        var getFormRequest = new MarketoRequest($"/rest/asset/v1/form/{input.FormId}.json", Method.Get, _credentials);
        var form = _client.ExecuteWithError<FormDto>(getFormRequest).Result.First();
        
        var getFieldsRequest = new MarketoRequest($"/rest/asset/v1/form/{input.FormId}/fields.json", Method.Get, _credentials);
        var formFields = _client.ExecuteWithError<FormFieldDto>(getFieldsRequest);
        var fieldsHtml = FormToHtmlConverter.ConvertToHtml(form, formFields.Result);
        var resultHtml = $"<html><body>{fieldsHtml}</body></html>";
        
        return new FileDataResponse
        {
            File = new File(Encoding.UTF8.GetBytes(resultHtml))
            {
                Name = $"{form.Name}.html",
                ContentType = MediaTypeNames.Text.Html
            }
        };
    }
}