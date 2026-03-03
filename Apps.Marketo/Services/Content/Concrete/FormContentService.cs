using Apps.Marketo.Dtos;
using Apps.Marketo.Dtos.Content;
using Apps.Marketo.Extensions;
using Apps.Marketo.Helper.FileFolder;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.HtmlHelpers.Forms;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Content.Response;
using Apps.Marketo.Models.Entities.Form;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;
using System.Net.Mime;
using System.Text;

namespace Apps.Marketo.Services.Content.Concrete;

public class FormContentService(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext), IContentService
{
    public async Task<FileReference> DownloadContent(DownloadContentRequest input)
    {
        var getFormRequest = new RestRequest($"/rest/asset/v1/form/{input.ContentId}.json", Method.Get);
        var form = await Client.ExecuteWithErrorHandlingFirst<FormEntity>(getFormRequest);

        var getFieldsRequest = new RestRequest($"/rest/asset/v1/form/{input.ContentId}/fields.json", Method.Get);
        var formFields = await Client.ExecuteWithErrorHandling<FormFieldDto>(getFieldsRequest);

        string resultHtml = FormToHtmlConverter.ConvertToHtml(
            form,
            formFields, 
            input.IgnoreVisibilityRules, 
            input.IgnoreFormFields);

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
