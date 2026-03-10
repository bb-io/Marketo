using Apps.Marketo.Constants;
using Apps.Marketo.Dtos;
using Apps.Marketo.Dtos.Content;
using Apps.Marketo.Extensions;
using Apps.Marketo.Helper.FileFolder;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.HtmlHelpers;
using Apps.Marketo.HtmlHelpers.Forms;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Content.Response;
using Apps.Marketo.Models.Entities;
using Apps.Marketo.Models.Entities.Form;
using Apps.Marketo.Services.Content.Models;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Mime;
using System.Text;

namespace Apps.Marketo.Services.Content.Concrete;

public class FormContentService(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext), IContentService
{
    public async Task UploadContent(UploadContentInput input)
    {
        var (formDto, formFields) = await HtmlToFormConverter.ConvertToForm(input.HtmlContent, Client);
        var metaTags = HtmlContentBuilder.ExtractAllMetaTags(input.HtmlContent);

        string formId = HtmlContentBuilder.GetRequiredMetaValue(
            input.ContentId, metaTags, MetadataConstants.BlackbirdFormId, "Form ID");

        var getFormRequest = new RestRequest($"/rest/asset/v1/form/{formId}.json", Method.Get);
        var existingForm = await Client.ExecuteWithErrorHandlingFirst<FormEntity>(getFormRequest);

        var updateSubmitButtonRequest = new RestRequest($"/rest/asset/v1/form/{formId}/submitButton.json", Method.Post);
        updateSubmitButtonRequest.AddParameter("label", formDto.ButtonLabel);
        updateSubmitButtonRequest.AddParameter("waitingLabel", formDto.WaitingLabel);
        await Client.ExecuteWithErrorHandling(updateSubmitButtonRequest);

        var validThankYouList = formDto.ThankYouList?
            .Where(t => !string.IsNullOrWhiteSpace(t.FollowupType) && !string.IsNullOrWhiteSpace(t.FollowupValue))
            .ToList();
        if (validThankYouList != null && validThankYouList.Count != 0)
        {
            foreach (var item in validThankYouList)
            {
                // Marketo panics when you send a double number, it can't parse it
                if (item.FollowupValue.EndsWith(".0"))
                    item.FollowupValue = item.FollowupValue.Replace(".0", "");
            }

            var updateThankYouListRequest = new RestRequest($"/rest/asset/v1/form/{formId}/thankYouPage.json", Method.Post);
            updateThankYouListRequest.AddParameter(
                "thankyou", 
                JsonConvert.SerializeObject(validThankYouList, JsonSettings.SerializerSettings));

            var updatedThankYouList = await Client.ExecuteWithErrorHandlingFirst<FormEntity>(updateThankYouListRequest);
            existingForm.ThankYouList = updatedThankYouList.ThankYouList;
        }

        foreach (var field in formFields)
        {
            var updateFieldRequest = new RestRequest($"/rest/asset/v1/form/{formId}/field/{field.Id}.json", Method.Post);

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
                    {
                        updateFieldRequest.AddParameter(
                            parameter.Key, 
                            JsonConvert.SerializeObject(parameter.Value, JsonSettings.SerializerSettings));
                    }
                    else
                        updateFieldRequest.AddParameter(parameter.Key, parameter.Value.ToString());
                }
            }

            await Client.ExecuteWithErrorHandling(updateFieldRequest);

            var addFieldVisibilityRequest = new RestRequest($"/rest/asset/v1/form/{formId}/field/{field.Id}/visibility.json", Method.Post);
            var visibilityRules = field.VisibilityRules;

            if (visibilityRules != null)
            {
                if (visibilityRules.Rules == null)
                {
                    addFieldVisibilityRequest.AddParameter(
                        "visibilityRule", 
                        JsonConvert.SerializeObject(new { ruleType = visibilityRules.RuleType }));
                }
                else
                {
                    addFieldVisibilityRequest.AddParameter("visibilityRule", JsonConvert.SerializeObject(new
                    {
                        ruleType = visibilityRules.RuleType,
                        rules = visibilityRules.Rules.Select(rule => new
                        {
                            rule.AltLabel,
                            rule.Operator,
                            rule.SubjectField,
                            rule.Values
                        })
                    }, JsonSettings.SerializerSettings));
                }
                await Client.ExecuteWithErrorHandling(addFieldVisibilityRequest);
            }
        }
    }

    public async Task<FileReference> DownloadContent(DownloadContentRequest input)
    {
        var getFormRequest = new RestRequest($"/rest/asset/v1/form/{input.ContentId}.json", Method.Get);
        var form = await Client.ExecuteWithErrorHandlingFirst<FormEntity>(getFormRequest);

        var getFieldsRequest = new RestRequest($"/rest/asset/v1/form/{input.ContentId}/fields.json", Method.Get);
        var formFields = await Client.ExecuteWithErrorHandling<FormFieldDto>(getFieldsRequest);

        string fieldsInnerHtml = FormToHtmlConverter.ConvertToInnerHtml(
            form, 
            formFields, 
            input.IgnoreVisibilityRules, 
            input.IgnoreFormFields); 
        
        var sections = new Dictionary<string, string> { { form.Id.ToString(), fieldsInnerHtml } };

        var metadata = new List<MetadataEntity> { new(MetadataConstants.BlackbirdFormId, form.Id.ToString()) };

        var resultHtml = HtmlContentBuilder.GenerateHtml(
            sections,
            form.Name,
            metadata);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        return await fileManagementClient.UploadAsync(stream, MediaTypeNames.Text.Html, form.Name.ToHtmlFileName());
    }

    public async Task<SearchContentResponse> SearchContent(SearchContentRequest input)
    {
        var request = new RestRequest($"/rest/asset/v1/forms.json", Method.Get);
        await FileFolderHelper.AddFolderParameter(Client, request, input.FolderId);
        request.AddQueryParameterIfNotNull("status", input.Status);

        var forms = await Client.Paginate<FormEntity>(request);

        forms = forms
            .ApplyDateAfterFilter(input.CreatedAfter, x => x.CreatedAt)
            .ApplyDateBeforeFilter(input.CreatedBefore, x => x.CreatedAt)
            .ApplyDateAfterFilter(input.UpdatedAfter, x => x.UpdatedAt)
            .ApplyDateBeforeFilter(input.UpdatedBefore, x => x.UpdatedAt)
            .ApplyNamePatternFilter(input.NamePatterns, input.ExcludeMatched, x => x.Name);

        forms = await forms.ApplyIgnoreInArchiveFilter(Client, input.IgnoreInArchive, x => x.Folder);

        return new(forms.Select(x => new ContentDto(x)).ToList());
    }
}
