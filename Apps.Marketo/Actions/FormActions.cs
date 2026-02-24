using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Apps.Marketo.Dtos;
using Apps.Marketo.Helper;
using Apps.Marketo.HtmlHelpers.Forms;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models;
using Apps.Marketo.Models.Forms.Requests;
using Apps.Marketo.Models.Forms.Responses;
using Apps.Marketo.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;
using System.Net.Mime;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Apps.Marketo.Actions;

[ActionList("Forms")]
public class FormActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) 
    : MarketoInvocable(invocationContext)
{
    [Action("Get form", Description = "Get specified form.")]
    public async Task<FormDto> GetForm([ActionParameter] FormIdentifier input)
    {
        var endpoint = $"/rest/asset/v1/form/{input.FormId}.json";
        var request = new RestRequest(endpoint, Method.Get);

        return await Client.ExecuteWithErrorHandlingFirst<FormDto>(request);
    }

    [Action("Search forms", Description = "Search all forms")]
    public async Task<ListFormsResponse> ListRecentlyCreatedOrUpdatedForms([ActionParameter] ListFormsRequest input)
    {
        var request = new RestRequest($"/rest/asset/v1/forms.json", Method.Get);
        await FileFolderHelper.AddFolderParameter(Client, request, input.FolderId);

        if (input.Status != null) 
            request.AddQueryParameter("status", input.Status);
        var forms = await Client.Paginate<FormDto>(request);
        if (input.EarliestUpdatedAt != null)
            forms = forms.Where(x => x.UpdatedAt >= input.EarliestUpdatedAt.Value).ToList();
        if (input.LatestUpdatedAt != null)
            forms = forms.Where(x => x.UpdatedAt <= input.LatestUpdatedAt.Value).ToList();

        forms = input.NamePatterns != null ? 
            forms.Where(x => FileFolderHelper.IsFilePathMatchingPattern(input.NamePatterns, x.Name, input.ExcludeMatched ?? false)).ToList() : 
            forms; 

        return new(forms);
    }

    [Action("Search forms fields", Description = "Search forms fields")]
    public ListFormFieldsResponse ListFormsFields(
        [ActionParameter] GetMultipleFormsRequest getMultipleFormsRequest, 
        [ActionParameter] ListFormFieldsRequest listFormFields)
    {
        return new ListFormFieldsResponse() { FormFieldsIds = listFormFields.FormFields.Select(x => x.Split(' ').First()).ToList() };
    }

    [Action("Update form metadata", Description = "Update form metadata")]
    public async Task<FormDto> UpdateEmailMetadata(
        [ActionParameter] FormIdentifier input,
        [ActionParameter] UpdateFormMetadataRequest updateFormMetadata)
    {
        var request = new RestRequest($"/rest/asset/v1/form/{input.FormId}.json", Method.Post);       
        if(!string.IsNullOrEmpty(updateFormMetadata.Name))
            request.AddParameter("name", updateFormMetadata.Name);
        if (!string.IsNullOrEmpty(updateFormMetadata.Description))
            request.AddParameter("description", updateFormMetadata.Description);
        return await Client.ExecuteWithErrorHandlingFirst<FormDto>(request);
    }

    [Action("Get form as HTML for translation", Description = "Retrieve a form as HTML file for translation.")]
    public async Task<FileWrapper> GetFormAsHtml(
        [ActionParameter] FormIdentifier input,
        [ActionParameter] IgnoreFieldsRequest ignoreFieldsRequest)
    {
        var getFormRequest = new RestRequest($"/rest/asset/v1/form/{input.FormId}.json", Method.Get);
        var form = await Client.ExecuteWithErrorHandlingFirst<FormDto>(getFormRequest);

        var getFieldsRequest = new RestRequest($"/rest/asset/v1/form/{input.FormId}/fields.json", Method.Get);
        var formFields = await Client.ExecuteWithErrorHandling<FormFieldDto>(getFieldsRequest);
        var fieldsHtml = FormToHtmlConverter.ConvertToHtml(form, formFields, ignoreFieldsRequest);
        var resultHtml = $"<html><body>{fieldsHtml}</body></html>";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        var file = fileManagementClient
            .UploadAsync(stream, MediaTypeNames.Text.Html, $"{form.Name.Replace(" ", "_")}.html").Result;
        return new() { File = file };
    }

    [Action("Create or update form from translated HTML", Description = "Create or update form from translated HTML.")]
    public async Task<FormDto> SetFormFromHtml(
        [ActionParameter] FileWrapper form,
        [ActionParameter] [DataSource(typeof(FormFolderDataHandler))] [Display("Folder")] string? folderId,
        [ActionParameter] UpdateFormRequest updateFormRequest)
    {
        var jsonSerializerSettings = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var formBytes = fileManagementClient.DownloadAsync(form.File).Result.GetByteData().Result;
        var html = Encoding.UTF8.GetString(formBytes);
        var (formDto, formFields) = HtmlToFormConverter.ConvertToForm(html, Client);
        
        object folder;

        if (folderId != null)
        {
            folder = new
            {
                Id = int.Parse(folderId.Split("_").First()),
                Type = folderId.Split("_").Last(),
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
            new RestRequest($"/rest/asset/v1/form/{formDto.Id}/clone.json", Method.Post)
                .AddParameter("name", string.IsNullOrWhiteSpace(updateFormRequest.FormName) 
                ? Path.GetFileNameWithoutExtension(form.File.Name) : updateFormRequest.FormName)
                .AddParameter("folder", JsonSerializer.Serialize(folder, jsonSerializerSettings))
                .AddParameter("description", formDto.Description);

        var cloneFormResult = await Client.ExecuteNoErrorHandling<FormDto>(cloneFormRequest);
        FormDto clonedForm;
        if (cloneFormResult?.Errors != null && cloneFormResult.Errors.Count != 0)
        {
            if (cloneFormResult.Errors.Any(x => x.Message.Contains("Form name already exists")))
            {
                await DeleteFormWithExistingName(string.IsNullOrWhiteSpace(updateFormRequest.FormName)
                    ? Path.GetFileNameWithoutExtension(form.File.Name)
                    : updateFormRequest.FormName);

                clonedForm = await Client.ExecuteWithErrorHandlingFirst<FormDto>(cloneFormRequest);
            }
            else
            {
                var errorMessages = string.Join("; ", cloneFormResult.Errors.Select(x => x.Message));
                throw new PluginApplicationException($"Failed to clone form: {errorMessages}");
            }
        }
        else
            clonedForm = cloneFormResult!.Result!.First();

        var updateSubmitButtonRequest = new RestRequest($"/rest/asset/v1/form/{clonedForm.Id}/submitButton.json", Method.Post);
        updateSubmitButtonRequest.AddParameter("label", formDto.ButtonLabel);
        updateSubmitButtonRequest.AddParameter("waitingLabel", formDto.WaitingLabel);
        clonedForm = await Client.ExecuteWithErrorHandlingFirst<FormDto>(updateSubmitButtonRequest);

        if (formDto.ThankYouList.Any())
        {
            var updateThankYouListRequest = new RestRequest(
                $"/rest/asset/v1/form/{clonedForm.Id}/thankYouPage.json",
                Method.Post);
            updateThankYouListRequest.AddParameter("thankyou",
                JsonSerializer.Serialize(formDto.ThankYouList, jsonSerializerSettings));
            var updatedThankYouList = await Client.ExecuteWithErrorHandlingFirst<FormDto>(updateThankYouListRequest);
            clonedForm.ThankYouList = updatedThankYouList.ThankYouList;
        }

        foreach (var field in formFields)
        {
            var updateFieldRequest = new RestRequest($"/rest/asset/v1/form/{clonedForm.Id}/field/{field.Id}.json",
                Method.Post);

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

            await Client.ExecuteWithErrorHandling(updateFieldRequest);

            var addFieldVisibilityRequest = new RestRequest(
                $"/rest/asset/v1/form/{clonedForm.Id}/field/{field.Id}/visibility.json",
                Method.Post);

            var visibilityRules = field.VisibilityRules;
            if (visibilityRules != null)
            {
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
                await Client.ExecuteWithErrorHandling(addFieldVisibilityRequest);
            }
        }

        return clonedForm;
    }

    private async Task DeleteFormWithExistingName(string formName)
    {
        var getFormByNameRequest = new RestRequest($"/rest/asset/v1/form/byName.json", Method.Get)
            .AddParameter("name", formName);
        var getFormByNameResponse = await Client.ExecuteWithErrorHandlingFirst<FormDto>(getFormByNameRequest);
        var deleteFormRequest = new RestRequest($"/rest/asset/v1/form/{getFormByNameResponse.Id}/delete.json", Method.Post);
        await Client.ExecuteWithErrorHandling(deleteFormRequest);
    }
}