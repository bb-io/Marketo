using System.Net.Mime;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Apps.Marketo.DataSourceHandlers;
using Apps.Marketo.Dtos;
using Apps.Marketo.HtmlHelpers.Forms;
using Apps.Marketo.Models;
using Apps.Marketo.Models.Forms.Requests;
using Apps.Marketo.Models.Forms.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;
using File = Blackbird.Applications.Sdk.Common.Files.File;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
            var updatedForms = response.Result.Where(form => form.UpdatedAt >= startDateTime 
                                                             && (input.FolderId == null 
                                                                 || form.Folder.Value == int.Parse(input.FolderId)));
            
            if (input.Status != null)
                updatedForms = updatedForms.Where(form => form.Status == input.Status);
            
            forms.AddRange(updatedForms);
            offset += maxReturn;
        } while (response.Result.Count == maxReturn);

        return new ListFormsResponse { Forms = forms };
    }

    [Action("Get form as HTML for translation", Description = "Retrieve a form as HTML file for translation.")]
    public FileWrapper GetFormAsHtml([ActionParameter] GetFormRequest input)
    {
        var getFormRequest = new MarketoRequest($"/rest/asset/v1/form/{input.FormId}.json", Method.Get, _credentials);
        var form = _client.ExecuteWithError<FormDto>(getFormRequest).Result.First();
        
        var getFieldsRequest = new MarketoRequest($"/rest/asset/v1/form/{input.FormId}/fields.json", Method.Get, _credentials);
        var formFields = _client.ExecuteWithError<FormFieldDto>(getFieldsRequest);
        var fieldsHtml = FormToHtmlConverter.ConvertToHtml(form, formFields.Result);
        var resultHtml = $"<html><body>{fieldsHtml}</body></html>";
        
        return new FileWrapper
        {
            File = new File(Encoding.UTF8.GetBytes(resultHtml))
            {
                Name = $"{form.Name.Replace(" ", "_")}.html",
                ContentType = MediaTypeNames.Text.Html
            }
        };
    }

    [Action("Create new form from translated HTML", Description = "Create a new form from translated HTML.")]
    public FormDto SetFormFromHtml([ActionParameter] FileWrapper form, 
        [ActionParameter] [DataSource(typeof(FolderDataHandler))] [Display("Folder")] string? folderId)
    {
        var jsonSerializerSettings = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var html = Encoding.UTF8.GetString(form.File.Bytes);
        var (formDto, formFields) = HtmlToFormConverter.ConvertToForm(html, _credentials);
        
        object folder;

        if (folderId != null)
        {
            var getFolderRequest = new MarketoRequest($"/rest/asset/v1/folder/{folderId}.json", Method.Get, _credentials);
            var folderDto = _client.ExecuteWithError<FolderInfoDto>(getFolderRequest).Result.First();
            folder = new
            {
                Id = int.Parse(folderDto.FolderId.Id),
                Type = folderDto.FolderId.Type
            };
        }
        else
        {
            folder = new
            {
                Id = formDto.Folder.Value,
                Type = formDto.Folder.Type
            };
        }
        
        var cloneFormRequest = new MarketoRequest($"/rest/asset/v1/form/{formDto.Id}/clone.json", Method.Post, _credentials);
        cloneFormRequest.AddParameter("name", Path.GetFileNameWithoutExtension(form.File.Name));
        cloneFormRequest.AddParameter("folder", JsonSerializer.Serialize(folder, jsonSerializerSettings));
        cloneFormRequest.AddParameter("description", formDto.Description);
        
        var clonedForm = _client.ExecuteWithError<FormDto>(cloneFormRequest).Result.First();

        var updateSubmitButtonRequest = new MarketoRequest($"/rest/asset/v1/form/{clonedForm.Id}/submitButton.json", 
            Method.Post, _credentials);
        updateSubmitButtonRequest.AddParameter("label", formDto.ButtonLabel);
        updateSubmitButtonRequest.AddParameter("waitingLabel", formDto.WaitingLabel);
        _client.ExecuteWithError(updateSubmitButtonRequest);
        
        var updateThankYouListRequest = new MarketoRequest($"/rest/asset/v1/form/{clonedForm.Id}/thankYouPage.json", 
            Method.Post, _credentials);
        updateThankYouListRequest.AddParameter("thankyou", JsonSerializer.Serialize(formDto.ThankYouList, jsonSerializerSettings));
        clonedForm = _client.ExecuteWithError<FormDto>(updateThankYouListRequest).Result.First();

        foreach (var field in formFields)
        {
            var updateFieldRequest = new MarketoRequest($"/rest/asset/v1/form/{clonedForm.Id}/field/{field.Id}.json", 
                Method.Post, _credentials);
            
            var fieldParameters = new Dictionary<string, object?>
            {
                { "defaultValue", field.DefaultValue },
                { "hintText", field.HintText },
                { "instructions", field.Instructions },
                { "label", field.Label },
                { "validationMessage", field.ValidationMessage },
                { "values", field.FieldMetaData?.Values }
            };
            
            foreach (var parameter in fieldParameters)
            {
                if (parameter.Value != null)
                {
                    if (parameter.Key == "values")
                        updateFieldRequest.AddParameter(parameter.Key, JsonSerializer.Serialize(parameter.Value, jsonSerializerSettings));
                    else
                        updateFieldRequest.AddParameter(parameter.Key, parameter.Value.ToString());
                }
            }

            _client.ExecuteWithError(updateFieldRequest);
            
            var addFieldVisibilityRequest = new MarketoRequest($"/rest/asset/v1/form/{clonedForm.Id}/field/{field.Id}/visibility.json", 
                Method.Post, _credentials);

            var visibilityRules = field.VisibilityRules;
            if (visibilityRules.Rules == null)
                addFieldVisibilityRequest.AddParameter("visibilityRule", JsonSerializer.Serialize(new { ruleType = visibilityRules.RuleType }));
            else
            {
                addFieldVisibilityRequest.AddParameter("visibilityRule", JsonSerializer.Serialize(new
                {
                    ruleType = visibilityRules.RuleType,
                    rules = visibilityRules.Rules.Select(rule => new
                    {
                        rule.AltLabel,
                        rule.Operator,
                        rule.SubjectField,
                        rule.Values
                    })
                }, jsonSerializerSettings));
            }

            _client.ExecuteWithError(addFieldVisibilityRequest);
        }
        
        return clonedForm;
    }
}