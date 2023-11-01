using System.Net.Mime;
using System.Text;
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
            var updatedForms = response.Result.Where(form => form.UpdatedAt >= startDateTime);
            
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
                Name = $"{form.Name}.html",
                ContentType = MediaTypeNames.Text.Html
            }
        };
    }

    [Action("Create new form from translated HTML", Description = "Create a new form from translated HTML.")]
    public FormDto SetFormFromHtml([ActionParameter] FileWrapper form, 
        [ActionParameter] [DataSource(typeof(FolderDataHandler))] string? folderId)
    {
        var jsonSerializerSettings = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var html = Encoding.UTF8.GetString(form.File.Bytes);
        var (formDto, formFields) = HtmlToFormConverter.ConvertToForm(html);
        
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

        var createFormRequest = new MarketoRequest("/rest/asset/v1/forms.json", Method.Post, _credentials);
        createFormRequest.AddParameter("name", Path.GetFileNameWithoutExtension(form.File.Name));
        createFormRequest.AddParameter("description", formDto.Description);
        createFormRequest.AddParameter("fontFamily", formDto.FontFamily);
        createFormRequest.AddParameter("fontSize", formDto.FontSize);
        createFormRequest.AddParameter("labelPosition", formDto.LabelPosition);
        createFormRequest.AddParameter("progressiveProfiling", formDto.ProgressiveProfiling);
        createFormRequest.AddParameter("folder", JsonSerializer.Serialize(folder, jsonSerializerSettings));

        var createdForm = _client.ExecuteWithError<FormDto>(createFormRequest).Result.First();
        
        var updateFormThemeRequest = new MarketoRequest($"/rest/asset/v1/form/{createdForm.Id}.json", Method.Post, 
            _credentials);
        updateFormThemeRequest.AddParameter("theme", formDto.Theme);
        _client.ExecuteWithError(updateFormThemeRequest);
        
        var updateSubmitButtonRequest = new MarketoRequest($"/rest/asset/v1/form/{createdForm.Id}/submitButton.json", 
            Method.Post, _credentials);
        updateSubmitButtonRequest.AddParameter("buttonPosition", formDto.ButtonLocation);
        updateSubmitButtonRequest.AddParameter("label", formDto.ButtonLabel);
        updateSubmitButtonRequest.AddParameter("waitingLabel", formDto.WaitingLabel);
        _client.ExecuteWithError(updateSubmitButtonRequest);
        
        var updateThankYouListRequest = new MarketoRequest($"/rest/asset/v1/form/{createdForm.Id}/submitButton.json", 
            Method.Post, _credentials);
        updateSubmitButtonRequest.AddParameter("thankyou", JsonSerializer.Serialize(formDto.ThankYouList, jsonSerializerSettings));
        createdForm = _client.ExecuteWithError<FormDto>(updateThankYouListRequest).Result.First();

        foreach (var field in formFields)
        {
            var addFieldRequest = new MarketoRequest($"/rest/asset/v1/form/{createdForm.Id}/fields.json", Method.Post,
                _credentials);
            
            var fieldParameters = new Dictionary<string, object?>
            {
                { "fieldId", field.Id },
                { "defaultValue", field.DefaultValue },
                { "fieldWidth", field.FieldWidth },
                { "formPrefill", field.FormPrefill },
                { "hintText", field.HintText },
                { "initiallyChecked", field.FieldMetaData?.InitiallyChecked },
                { "instructions", field.Instructions },
                { "label", field.Label },
                { "labelWidth", field.LabelWidth },
                { "maskInput", field.FieldMetaData?.FieldMask },
                { "maxLength", field.MaxLength },
                { "maxValue", field.FieldMetaData?.MaxValue },
                { "minValue", field.FieldMetaData?.MinValue },
                { "multiSelect", field.FieldMetaData?.MultiSelect },
                { "required", field.Required },
                { "validationMessage", field.ValidationMessage },
                { "values", field.FieldMetaData?.Values },
                { "visibleLines", field.FieldMetaData?.VisibleLines }
            };
            
            foreach (var parameter in fieldParameters)
            {
                if (parameter.Value != null)
                    addFieldRequest.AddParameter(parameter.Key, parameter.Value.ToString());
            }

            var createdField = _client.ExecuteWithError<FormFieldDto>(addFieldRequest).Result.First();
            
            var addFieldVisibilityRequest = new MarketoRequest($"/rest/asset/v1/form/{createdForm.Id}/field/{createdField.Id}/visibility.json", 
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
        
        return createdForm;
    }
}