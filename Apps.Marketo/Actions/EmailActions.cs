using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Emails.Requests;
using Apps.Marketo.Models.Emails.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;
using System.Globalization;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Snippets.Request;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Web;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using System.Net.Mime;
using System.Text;
using Apps.Marketo.Models;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Apps.Marketo.HtmlHelpers;

namespace Apps.Marketo.Actions;

[ActionList]
public class EmailActions : MarketoInvocable
{
    private const string HtmlIdAttribute = "id";

    private readonly IFileManagementClient _fileManagementClient;

    public EmailActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("Search emails", Description = "Search all emails")]
    public ListEmailsResponse ListEmails([ActionParameter] ListEmailsRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/emails.json", Method.Get, Credentials);
        AddFolderParameter(request, input.FolderId);

        if (input.Status != null) request.AddQueryParameter("status", input.Status);   
        if (input.EarliestUpdatedAt != null)
            request.AddQueryParameter("earliestUpdatedAt",
                ((DateTime)input.EarliestUpdatedAt).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        if (input.LatestUpdatedAt != null)
            request.AddQueryParameter("latestUpdatedAt",
                ((DateTime)input.LatestUpdatedAt).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));

        var response = Client.Paginate<EmailDto>(request);
        response = input.NamePatterns != null ? response.Where(x => IsFilePathMatchingPattern(input.NamePatterns, x.Name, input.ExcludeMatched ?? false)).ToList() : response;
        return new() { Emails = response };
    }

    [Action("Get email info", Description = "Get email info")]
    public EmailDto GetEmailInfo([ActionParameter] GetEmailInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}.json", Method.Get, Credentials);
        return Client.GetSingleEntity<EmailDto>(request);
    }

    [Action("Update email metadata", Description = "Update email metadata")]
    public EmailDto UpdateEmailMetadata(
        [ActionParameter] GetEmailInfoRequest input,
        [ActionParameter] UpdateEmailMetadataRequest updateEmailMetadata)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}.json", Method.Post, Credentials);
        if (!string.IsNullOrEmpty(updateEmailMetadata.Name))
            request.AddParameter("name", updateEmailMetadata.Name);
        if (!string.IsNullOrEmpty(updateEmailMetadata.Description))
            request.AddParameter("description", updateEmailMetadata.Description);
        return Client.GetSingleEntity<EmailDto>(request);
    }

    [Action("Get email content", Description = "Get email content")]
    public EmailContentUserFriendlyResponse GetEmailContent([ActionParameter] GetEmailInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}/content.json", Method.Get, Credentials);
        var response = Client.ExecuteWithError<EmailContentDto>(request);
        return new(response.Result);
    }

    [Action("Delete email", Description = "Delete email")]
    public void DeleteEmail([ActionParameter] GetEmailInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}/delete.json", Method.Post, Credentials);
        Client.ExecuteWithError<IdDto>(request);
    }

    [Action("Get email as HTML for translation", Description = "Get email as HTML for translation")]
    public async Task<FileWrapper> GetEmailAsHtml(
        [ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
        [ActionParameter] GetSegmentationRequest getSegmentationRequest,
        [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest)
    {
        var emailInfo = GetEmailInfo(getEmailInfoRequest);
        var emailContentResponse = GetEmailContentAll(getEmailInfoRequest);
        var sectionContent = emailContentResponse.EmailContentItems!
            .Where(x => x.ContentType == "DynamicContent" || x.ContentType == "Text")
            .ToDictionary(
                x => x.HtmlId, 
                y => GetEmailSectionContent(getEmailInfoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, y));
        var resultHtml = HtmlContentBuilder.GenerateHtml(sectionContent, emailInfo.Name, getSegmentBySegmentationRequest.Segment);
        
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        var file = await _fileManagementClient.UploadAsync(stream, MediaTypeNames.Text.Html, $"{emailInfo.Name}.html");
        return new() { File = file };
    }

    [Action("Translate email from HTML file", Description = "Translate email from HTML file")]
    public void TranslateEmailWithHtml(
        [ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
        [ActionParameter] GetSegmentationRequest getSegmentationRequest,
        [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        [ActionParameter] TranslateEmailWithHtmlRequest translateEmailWithHtmlRequest)
    {
        var emailContentResponse = GetEmailContentAll(getEmailInfoRequest);

        if (!translateEmailWithHtmlRequest.TranslateOnlyDynamic.HasValue || 
            !translateEmailWithHtmlRequest.TranslateOnlyDynamic.Value)
        {
            foreach (var item in emailContentResponse.EmailContentItems)
            {
                if (item.ContentType == "Text")
                {
                    ConvertSectionToDynamicContent(getEmailInfoRequest.EmailId, item.HtmlId, getSegmentationRequest.SegmentationId);
                }
            }
            emailContentResponse = GetEmailContentAll(getEmailInfoRequest);
        }
        var translatedContent = HtmlContentBuilder.ParseHtml(translateEmailWithHtmlRequest.File, _fileManagementClient);
        var modulesToIgnore = new List<string>();
        foreach (var item in emailContentResponse.EmailContentItems)
        {
            if (item.ContentType == "DynamicContent" && !modulesToIgnore.Contains(item.ParentHtmlId) && translatedContent.TryGetValue(item.HtmlId, out var translatedContentItem))
            {
                var ignoreModule = UpdateEmailDynamicContent(getEmailInfoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, item, translatedContentItem, emailContentResponse.EmailContentItems, translatedContent, 0);
                modulesToIgnore.Add(ignoreModule);
            }
        }
    }

    private IdDto ConvertSectionToDynamicContent(string emailId, string htmlId, string segmentationId)
    {
        var endpoint = $"/rest/asset/v1/email/{emailId}/content/{htmlId}.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddParameter("value", segmentationId)
            .AddParameter("type", "DynamicContent");
        return Client.ExecuteWithError<IdDto>(request).Result.FirstOrDefault();
    }

    private string UpdateEmailDynamicContent(
        GetEmailInfoRequest getEmailInfoRequest,
        GetSegmentationRequest getSegmentationRequest,
        GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        EmailContentDto dynamicContentItem,
        string content,
        List<EmailContentDto> emailContentItems,
        Dictionary<string, string> translatedContent,
        int tryNumber)
    {
        var endpoint =
            $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{dynamicContentItem.Value.ToString()}.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddQueryParameter("segment", getSegmentBySegmentationRequest.Segment)
            .AddQueryParameter("type", "HTML")
            .AddQueryParameter("value", content);
        try
        {
            Client.GetSingleEntity<IdDto>(request);
            return string.Empty;
        }
        catch (BusinessRuleViolationException ex) 
        {
            if(ex.ErrorCode == 611 && tryNumber == 0)
            {
                var ignoreModuleId = RecreateModuleWithIssue(getEmailInfoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, dynamicContentItem, emailContentItems, translatedContent, tryNumber);
                return ignoreModuleId;
            }
            throw ex;
        }
    }

    private string GetEmailSectionContent(
        GetEmailInfoRequest getEmailInfoRequest,
        GetSegmentationRequest getSegmentationRequest,
        GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        EmailContentDto sectionContent)
    {
        if (sectionContent.ContentType == "DynamicContent")
        {
            var requestSeg = new MarketoRequest(
                    $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{sectionContent.Value.ToString()}.json",
                    Method.Get, Credentials);

            var responseSeg = Client.ExecuteWithError<DynamicContentDto>(requestSeg);
            if (responseSeg.Result!.First().Segmentation.ToString() == getSegmentationRequest.SegmentationId)
                return responseSeg.Result!.First().Content
                    .Where(x => x.Type == "HTML" && x.SegmentName == getSegmentBySegmentationRequest.Segment)
                    .Select(x => new GetEmailDynamicContentResponse(x)
                    { DynamicContentId = sectionContent.Value.ToString()! }).First().Content;
        }
        else if(sectionContent.ContentType == "Text")
        {
            return JsonConvert.DeserializeObject<List<EmailContentValueDto>>(sectionContent.Value.ToString()).First(x => x.Type == "HTML").Value;
        }
        return string.Empty;
    }

    private EmailContentResponse GetEmailContentAll([ActionParameter] GetEmailInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}/content.json", Method.Get, Credentials);
        var response = Client.ExecuteWithError<EmailContentDto>(request);
        return new(response.Result);
    }

    private string RecreateModuleWithIssue(
        GetEmailInfoRequest getEmailInfoRequest,
        GetSegmentationRequest getSegmentationRequest,
        GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        EmailContentDto dynamicContentItem, 
        List<EmailContentDto> emailContentItems,
        Dictionary<string, string> translatedContent,
        int tryNumber)
    {
        var parentModule = emailContentItems.FirstOrDefault(x => x.HtmlId == dynamicContentItem.ParentHtmlId);
        var allOldTextItemsInModule = emailContentItems.Where(x => x.ParentHtmlId == dynamicContentItem.ParentHtmlId && x.ContentType == "Text").ToList();

        var createDuplicateRequest = new MarketoRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content/{dynamicContentItem.ParentHtmlId}/duplicate.json", Method.Post, Credentials);
        var createDuplicateResponse = Client.GetSingleEntity<IdDto>(createDuplicateRequest);

        var deleteOldDuplicateRequest = new MarketoRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content/{dynamicContentItem.ParentHtmlId}/delete.json", Method.Post, Credentials);
        var deleteOldDuplicateResponse = Client.GetSingleEntity<IdDto>(deleteOldDuplicateRequest);

        
        var emailContentResponse = GetEmailContentAll(getEmailInfoRequest);

        var newModule = emailContentResponse.EmailContentItems.FirstOrDefault(x => x.Index == parentModule.Index);
        var itemsInNewModule = emailContentResponse.EmailContentItems.Where(x => x.ParentHtmlId == newModule.HtmlId && x.ContentType == "Text").ToList();

        foreach (var item in itemsInNewModule)
        {
            ConvertSectionToDynamicContent(getEmailInfoRequest.EmailId, item.HtmlId, getSegmentationRequest.SegmentationId);   
        }

        emailContentResponse = GetEmailContentAll(getEmailInfoRequest);
        itemsInNewModule = emailContentResponse.EmailContentItems.Where(x => x.ParentHtmlId == newModule.HtmlId && x.ContentType == "Text").ToList();

        int textItemsCounter = 0;
        foreach(var dynamicItem in itemsInNewModule)
        {
            translatedContent.TryGetValue(allOldTextItemsInModule.ElementAt(textItemsCounter).HtmlId, out var translatedContentItem);
            UpdateEmailDynamicContent(getEmailInfoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, dynamicItem, translatedContentItem, null, null, ++tryNumber);
            ++textItemsCounter;
        }
        return parentModule.HtmlId;
    }

    // Deprecated
    // Actions for more general usage of dynamic content

    //[Action("Update email dynamic content", Description = "Update email dynamic content")]
    //public IdDto UpdateEmailDynamicContent([ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
    //    [ActionParameter] GetEmailDynamicItemRequest getEmailDynamicItemRequest,
    //    [ActionParameter] GetSegmentationRequest getSegmentationRequest,
    //    [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
    //    [ActionParameter] UpdateEmailDynamicContentRequest updateEmailDynamicContentRequest)
    //{
    //    var endpoint =
    //        $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{getEmailDynamicItemRequest.DynamicContentId}.json";
    //    var request = new MarketoRequest(endpoint, Method.Post, Credentials)
    //        .AddQueryParameter("segment", getSegmentBySegmentationRequest.Segment)
    //        .AddQueryParameter("type", "HTML")
    //        .AddQueryParameter("value", updateEmailDynamicContentRequest.HTMLContent);

    //    return Client.GetSingleEntity<IdDto>(request);
    //}

    //[Action("Get email dynamic content", Description = "Get email dynamic content")]
    //public GetEmailDynamicContentResponse GetEmailDynamicContent(
    //    [ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
    //    [ActionParameter] GetEmailDynamicItemRequest getEmailDynamicItemRequest,
    //    [ActionParameter] GetEmailSegmentRequest getSegmentRequest)
    //{
    //    var endpoint =
    //        $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{getEmailDynamicItemRequest.DynamicContentId}.json";
    //    var request = new MarketoRequest(endpoint, Method.Get, Credentials);
    //    var response = Client.GetSingleEntity<DynamicContentDto>(request);

    //    var dynamicContent =
    //        response.Content.FirstOrDefault(x => x.Type == "HTML" && x.SegmentName == getSegmentRequest.Segment);
    //    return new(dynamicContent)
    //    {
    //        DynamicContentId = getEmailDynamicItemRequest.DynamicContentId
    //    };
    //}

    //[Action("List email dynamic content", Description = "List email dynamic content by segmentation")]
    //public ListEmailDynamicContentResponse ListEmailDynamicContent(
    //    [ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
    //    [ActionParameter] GetSegmentationRequest getSegmentationRequest,
    //    [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest)
    //{
    //    var endpoint = $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content.json";
    //    var request = new MarketoRequest(endpoint, Method.Get, Credentials)
    //        .AddQueryParameter("maxReturn", 200);

    //    var response = Client.ExecuteWithError<EmailContentDto>(request);
    //    var allDynamicContentInfo = response.Result.Where(e => e.ContentType == "DynamicContent").ToList();

    //    var result = new List<GetEmailDynamicContentResponse>();
    //    foreach (var dynamicContentInfo in allDynamicContentInfo)
    //    {
    //        var requestSeg =
    //            new MarketoRequest(
    //                $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{dynamicContentInfo.Value.ToString()}.json",
    //                Method.Get, Credentials);
    //        var responseSeg = Client.ExecuteWithError<DynamicContentDto>(requestSeg);
    //        if (responseSeg.Result!.First().Segmentation.ToString() == getSegmentationRequest.SegmentationId)
    //            result.Add(responseSeg.Result!.First().Content
    //                .Where(x => x.Type == "HTML" && x.SegmentName == getSegmentBySegmentationRequest.Segment)
    //                .Select(x => new GetEmailDynamicContentResponse(x)
    //                    { DynamicContentId = dynamicContentInfo.Value.ToString()! }).FirstOrDefault()!);
    //    }
    //    return new() { EmailDynamicContentList = result };
    //}

    //[Action("Update email content", Description = "Update content of a specific email")]
    //public void UpdateEmailContent(
    //    [ActionParameter] GetEmailInfoRequest emailRequest,
    //    [ActionParameter] UpdateContentRequest input)
    //{
    //    var request = new MarketoRequest($"/rest/asset/v1/email/{emailRequest.EmailId}/content.json", Method.Post,
    //            Credentials)
    //        .AddParameter("type", input.Type)
    //        .AddParameter("content", input.Content);

    //    Client.ExecuteWithErrorHandling(request);
    //}
}