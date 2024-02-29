using System.Net.Mime;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Apps.Marketo.DataSourceHandlers;
using Apps.Marketo.Dtos;
using Apps.Marketo.HtmlHelpers.Forms;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models;
using Apps.Marketo.Models.Forms.Requests;
using Apps.Marketo.Models.Forms.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using RestSharp;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Apps.Marketo.Actions;

[ActionList]
public class FormActions : MarketoInvocable
{
    private readonly IFileManagementClient _fileManagementClient;

    public FormActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
        : base(invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("Get form", Description = "Get specified form.")]
    public FormDto GetForm([ActionParameter] GetFormRequest input)
    {
        var endpoint = $"/rest/asset/v1/form/{input.FormId}.json";
        var request = new MarketoRequest(endpoint, Method.Get, Credentials);

        return Client.GetSingleEntity<FormDto>(request);
    }

    [Action("Search forms created or updated in date range", Description = "Search all forms that have been created or " +
                                                                         "updated in the specified date range. If end " +
                                                                         "date is not specified, it is set to current date.")]
    public ListFormsResponse ListRecentlyCreatedOrUpdatedForms([ActionParameter] ListFormsRequest input)
    {
        var endDateTime = input.EndDate ?? DateTime.Now.ToUniversalTime();
        var startDateTime = input.StartDate.ToUniversalTime();
        var offset = 0;
        var maxReturn = 200;
        var forms = new List<FormDto>();
        BaseResponseDto<FormDto> response;

        do
        {
            var request = new MarketoRequest($"/rest/asset/v1/forms.json?maxReturn={maxReturn}&offset={offset}",
                Method.Get, Credentials);
            response = Client.ExecuteWithError<FormDto>(request);
            var updatedForms = response.Result.Where(form => form.UpdatedAt >= startDateTime
                                                             && form.UpdatedAt <= endDateTime
                                                             && (input.FolderId == null
                                                                 || form.Folder.Value == int.Parse(input.FolderId)));

            if (input.Status != null)
                updatedForms = updatedForms.Where(form => form.Status == input.Status);

            forms.AddRange(updatedForms);
            offset += maxReturn;
        } while (response.Result.Count == maxReturn);

        return new() { Forms = forms };
    }

    [Action("Get form as HTML for translation", Description = "Retrieve a form as HTML file for translation.")]
    public async Task<FileWrapper> GetFormAsHtml([ActionParameter] GetFormRequest input)
    {
        var getFormRequest = new MarketoRequest($"/rest/asset/v1/form/{input.FormId}.json", Method.Get, Credentials);
        var form = Client.GetSingleEntity<FormDto>(getFormRequest);

        var getFieldsRequest =
            new MarketoRequest($"/rest/asset/v1/form/{input.FormId}/fields.json", Method.Get, Credentials);
        var formFields = Client.ExecuteWithError<FormFieldDto>(getFieldsRequest);
        var fieldsHtml = FormToHtmlConverter.ConvertToHtml(form, formFields.Result);
        var resultHtml = $"<html><body>{fieldsHtml}</body></html>";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        var file = _fileManagementClient
            .UploadAsync(stream, MediaTypeNames.Text.Html, $"{form.Name.Replace(" ", "_")}.html").Result;
        return new() { File = file };
    }

    [Action("Create new form from translated HTML", Description = "Create a new form from translated HTML.")]
    public FormDto SetFormFromHtml([ActionParameter] FileWrapper form,
        [ActionParameter] [DataSource(typeof(FolderDataHandler))] [Display("Folder")]
        string? folderId)
    {
        var jsonSerializerSettings = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var formBytes = _fileManagementClient.DownloadAsync(form.File).Result.GetByteData().Result;
        var html = Encoding.UTF8.GetString(formBytes);
        var (formDto, formFields) = HtmlToFormConverter.ConvertToForm(html, Credentials);

        object folder;

        if (folderId != null)
        {
            var getFolderRequest =
                new MarketoRequest($"/rest/asset/v1/folder/{folderId}.json", Method.Get, Credentials);
            var folderDto = Client.GetSingleEntity<FolderInfoDto>(getFolderRequest);
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

        var cloneFormRequest =
            new MarketoRequest($"/rest/asset/v1/form/{formDto.Id}/clone.json", Method.Post, Credentials)
                .AddParameter("name", Path.GetFileNameWithoutExtension(form.File.Name))
                .AddParameter("folder", JsonSerializer.Serialize(folder, jsonSerializerSettings))
                .AddParameter("description", formDto.Description);

        var clonedForm = Client.GetSingleEntity<FormDto>(cloneFormRequest);

        var updateSubmitButtonRequest = new MarketoRequest($"/rest/asset/v1/form/{clonedForm.Id}/submitButton.json",
            Method.Post, Credentials);
        updateSubmitButtonRequest.AddParameter("label", formDto.ButtonLabel);
        updateSubmitButtonRequest.AddParameter("waitingLabel", formDto.WaitingLabel);
        clonedForm = Client.GetSingleEntity<FormDto>(updateSubmitButtonRequest);

        var updateThankYouListRequest = new MarketoRequest($"/rest/asset/v1/form/{clonedForm.Id}/thankYouPage.json",
            Method.Post, Credentials);
        updateThankYouListRequest.AddParameter("thankyou",
            JsonSerializer.Serialize(formDto.ThankYouList, jsonSerializerSettings));
        var updatedThankYouList = Client.GetSingleEntity<FormDto>(updateThankYouListRequest);
        clonedForm.ThankYouList = updatedThankYouList.ThankYouList;

        foreach (var field in formFields)
        {
            var updateFieldRequest = new MarketoRequest($"/rest/asset/v1/form/{clonedForm.Id}/field/{field.Id}.json",
                Method.Post, Credentials);

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
                        updateFieldRequest.AddParameter(parameter.Key,
                            JsonSerializer.Serialize(parameter.Value, jsonSerializerSettings));
                    else
                        updateFieldRequest.AddParameter(parameter.Key, parameter.Value.ToString());
                }
            }

            Client.ExecuteWithErrorHandling(updateFieldRequest);

            var addFieldVisibilityRequest = new MarketoRequest(
                $"/rest/asset/v1/form/{clonedForm.Id}/field/{field.Id}/visibility.json",
                Method.Post, Credentials);

            var visibilityRules = field.VisibilityRules;
            if (visibilityRules.Rules == null)
                addFieldVisibilityRequest.AddParameter("visibilityRule",
                    JsonSerializer.Serialize(new { ruleType = visibilityRules.RuleType }));
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

            Client.ExecuteWithErrorHandling(addFieldVisibilityRequest);
        }

        return clonedForm;
    }
}