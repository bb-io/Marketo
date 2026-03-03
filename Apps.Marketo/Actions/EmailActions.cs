using Apps.Marketo.Constants;
using Apps.Marketo.Dtos;
using Apps.Marketo.Dtos.Email;
using Apps.Marketo.Extensions;
using Apps.Marketo.Helper.FileFolder;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Emails.Requests;
using Apps.Marketo.Models.Emails.Responses;
using Apps.Marketo.Models.Entities.Email;
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

[ActionList("Emails")]
public class EmailActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext)
{
    private readonly ContentServiceFactory _factory = new(invocationContext, fileManagementClient);

    [Action("Search emails", Description = "Search all emails")]
    public async Task<SearchEmailsResponse> ListEmails([ActionParameter] SearchEmailsRequest input)
    {
        input.Validate();

        var request = new RestRequest($"/rest/asset/v1/emails.json", Method.Get);
        var subfolders = await FileFolderHelper.AddFolderParameter(Client, request, input.FolderId);
        request
            .AddQueryParameterIfNotNull("status", input.Status)       
            .AddQueryParameterIfNotNull("earliestUpdatedAt", input.UpdatedAfter)
            .AddQueryParameterIfNotNull("latestUpdatedAt", input.UpdatedBefore);

        var emails = await Client.Paginate<EmailEntity>(request);

        emails = emails
            .ApplyDateAfterFilter(input.CreatedAfter, x => x.CreatedAt)
            .ApplyDateBeforeFilter(input.CreatedBefore, x => x.CreatedAt)
            .ApplyNamePatternFilter(input.NamePatterns, input.ExcludeMatched, x => x.Name);

        emails = await emails.ApplyIgnoreInArchiveFilter(Client, input.IgnoreInArchive, x => x.Folder);

        return new(emails.Select(x => new EmailDto(x)).ToList());
    }

    [Action("Get email info", Description = "Get email info")]
    public async Task<EmailDto> GetEmailInfo([ActionParameter] EmailIdentifier input)
    {
        var request = new RestRequest($"/rest/asset/v1/email/{input.EmailId}.json", Method.Get);
        var result = await Client.ExecuteWithErrorHandlingFirst<EmailEntity>(request);
        return new(result);
    }

    [Action("Update email metadata", Description = "Update email metadata")]
    public async Task<EmailDto> UpdateEmailMetadata(
        [ActionParameter] EmailIdentifier input,
        [ActionParameter] UpdateEmailMetadataRequest updateEmailMetadata)
    {
        var request = new RestRequest($"/rest/asset/v1/email/{input.EmailId}.json", Method.Post)
            .AddParameterIfNotNull("name", updateEmailMetadata.Name)
            .AddParameterIfNotNull("description", updateEmailMetadata.Description);

        var result = await Client.ExecuteWithErrorHandlingFirst<EmailEntity>(request);
        return new(result);
    }

    [Action("Get email content", Description = "Get email content")]
    public async Task<EmailContentUserFriendlyResponse> GetEmailContent([ActionParameter] EmailIdentifier input)
    {
        var request = new RestRequest($"/rest/asset/v1/email/{input.EmailId}/content.json", Method.Get);
        var response = await Client.ExecuteWithErrorHandling<EmailContentDto>(request);
        return new(response.ToList());
    }

    [Action("Delete email", Description = "Delete email")]
    public async Task DeleteEmail([ActionParameter] EmailIdentifier input)
    {
        var request = new RestRequest($"/rest/asset/v1/email/{input.EmailId}/delete.json", Method.Post);
        await Client.ExecuteWithErrorHandling<IdDto>(request);
    }

    [Action("Download email content", Description = "Download email content")]
    public async Task<DownloadEmailResponse> GetEmailAsHtml(
        [ActionParameter] EmailIdentifier emailInput,
        [ActionParameter] DownloadEmailRequest downloadInput)
    {
        var service = _factory.GetContentService(ContentTypes.Email);
        var input = new DownloadContentRequest
        {
            ContentId = emailInput.EmailId,
            Segment = downloadInput.Segment,
            SegmentationId = downloadInput.SegmentationId,
            GetOnlyDynamicContent = downloadInput.GetOnlyDynamicContent,
            IncludeImages = downloadInput.IncludeImages,
        };

        var file = await service.DownloadContent(input);
        return new(file);
    }

    [Action("Upload email content", Description = "Upload email content")]
    public async Task TranslateEmailWithHtml(
        [ActionParameter] OptionalEmailIdenfitier emailInput,
        [ActionParameter] UploadEmailRequest uploadInput)
    {
        string html = await ContentDownloader.DownloadHtmlContent(fileManagementClient, uploadInput.File);
        var input = new UploadContentInput(html, emailInput, uploadInput);

        var service = _factory.GetContentService(ContentTypes.Email);
        await service.UploadContent(input);
    }

    [Action("Get email dynamic content", Description = "Get email dynamic content")]
    public async Task<DynamicContentDto<EmailBaseSegmentDto>> GetEmailDynamicContent(
        [ActionParameter] EmailIdentifier getEmailInfoRequest,
        [ActionParameter] GetEmailDynamicItemRequest getEmailDynamicItemRequest,
        [ActionParameter] GetEmailSegmentRequest getSegmentRequest)
    {
        var endpoint =
            $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{getEmailDynamicItemRequest.DynamicContentId}.json";
        var request = new RestRequest(endpoint, Method.Get);
        return await Client.ExecuteWithErrorHandlingFirst<DynamicContentDto<EmailBaseSegmentDto>>(request);
    }

    [Action("Get email dynamic image content", Description = "Get email dynamic image content")]
    public async Task<DynamicContentDto<EmailImageSegmentDto>> GetEmailDynamicImageContent(
       [ActionParameter] EmailIdentifier getEmailInfoRequest,
       [ActionParameter] GetEmailDynamicItemRequest getEmailDynamicItemRequest,
       [ActionParameter] GetEmailSegmentRequest getSegmentRequest)
    {
        var endpoint =
            $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{getEmailDynamicItemRequest.DynamicContentId}.json";
        var request = new RestRequest(endpoint, Method.Get);
        return await Client.ExecuteWithErrorHandlingFirst<DynamicContentDto<EmailImageSegmentDto>>(request);
    }
}