using Apps.Marketo.Constants;
using Apps.Marketo.Dtos.Form;
using Apps.Marketo.Extensions;
using Apps.Marketo.Helper.FileFolder;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Entities.Form;
using Apps.Marketo.Models.Forms.Requests;
using Apps.Marketo.Models.Forms.Responses;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.Identifiers.Optional;
using Apps.Marketo.Services.Content;
using Apps.Marketo.Services.Content.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;

namespace Apps.Marketo.Actions;

[ActionList("Forms")]
public class FormActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) 
    : MarketoInvocable(invocationContext)
{
    private readonly ContentServiceFactory _factory = new(invocationContext, fileManagementClient);

    [Action("Get form", Description = "Get information of a specific form")]
    public async Task<FormDto> GetForm([ActionParameter] FormIdentifier input)
    {
        var endpoint = $"/rest/asset/v1/form/{input.FormId}.json";
        var request = new RestRequest(endpoint, Method.Get);
        var result = await Client.ExecuteWithErrorHandlingFirst<FormEntity>(request);
        return new(result);
    }

    [Action("Search forms", Description = "Search forms using specific criteria")]
    public async Task<SearchFormsResponse> ListRecentlyCreatedOrUpdatedForms([ActionParameter] SearchFormsRequest input)
    {
        input.Validate();

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

        return new(forms.Select(x => new FormDto(x)).ToList());
    }

    [Action("Search forms fields", Description = "Search fields of a specific form")]
    public ListFormFieldsResponse ListFormsFields(
        [ActionParameter] GetMultipleFormsRequest getMultipleFormsRequest, 
        [ActionParameter] ListFormFieldsRequest listFormFields)
    {
        return new ListFormFieldsResponse() { FormFieldsIds = listFormFields.FormFields.Select(x => x.Split(' ').First()).ToList() };
    }

    [Action("Update form metadata", Description = "Update metadata of a specific form")]
    public async Task<FormDto> UpdateEmailMetadata(
        [ActionParameter] FormIdentifier input,
        [ActionParameter] UpdateFormMetadataRequest updateFormMetadata)
    {
        var request = new RestRequest($"/rest/asset/v1/form/{input.FormId}.json", Method.Post)
            .AddParameterIfNotNull("name", updateFormMetadata.Name)
            .AddParameterIfNotNull("description", updateFormMetadata.Description);

        var result = await Client.ExecuteWithErrorHandlingFirst<FormEntity>(request);
        return new(result);
    }

    [Action("Download form content", Description = "Download content of a specific form")]
    public async Task<DownloadFormResponse> GetFormAsHtml(
        [ActionParameter] FormIdentifier formInput,
        [ActionParameter] DownloadFormRequest downloadInput)
    {
        var service = _factory.GetContentService(ContentTypes.Form);
        var input = new DownloadContentRequest
        {
            ContentId = formInput.FormId,
            IgnoreFormFields = downloadInput.IgnoreFields,
            IgnoreVisibilityRules = downloadInput.IgnoreVisibilityRules,
        };

        var file = await service.DownloadContent(input);
        return new(file);
    }

    [Action("Upload form content", Description = "Upload content of a specific form")]
    public async Task UploadFormContent(
        [ActionParameter] OptionalFormIdenfitier formInput,
        [ActionParameter] UploadFormRequest uploadInput)
    {
        string html = await ContentDownloader.DownloadHtmlContent(fileManagementClient, uploadInput.File);
        var input = new UploadContentInput(html, formInput, uploadInput);

        var service = _factory.GetContentService(ContentTypes.Form);
        await service.UploadContent(input);
    }
}