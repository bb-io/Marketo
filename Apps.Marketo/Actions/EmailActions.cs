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

namespace Apps.Marketo.Actions;

[ActionList]
public class EmailActions : MarketoInvocable
{
    private const string LanguageAttribute = "lang";
    private const string HtmlIdAttribute = "id";

    private readonly IFileManagementClient _fileManagementClient;

    public EmailActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("List emails", Description = "List all emails")]
    public ListEmailsResponse ListEmails([ActionParameter] ListEmailsRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/emails.json", Method.Get, Credentials);
        if (input.Status != null) request.AddQueryParameter("status", input.Status);
        if (input.FolderId != null)
            request.AddQueryParameter("folder",
                JsonConvert.SerializeObject(new { id = int.Parse(input.FolderId), type = input.Type ?? "Folder" }));
        if (input.EarliestUpdatedAt != null)
            request.AddQueryParameter("earliestUpdatedAt",
                ((DateTime)input.EarliestUpdatedAt).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        if (input.LatestUpdatedAt != null)
            request.AddQueryParameter("latestUpdatedAt",
                ((DateTime)input.LatestUpdatedAt).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));

        var response = Client.Paginate<EmailDto>(request);
        return new() { Emails = response };
    }

    [Action("Get email info", Description = "Get email info")]
    public EmailDto GetEmailInfo([ActionParameter] GetEmailInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}.json", Method.Get, Credentials);
        return Client.GetSingleEntity<EmailDto>(request);
    }

    [Action("Get email content", Description = "Get email content")]
    public EmailContentResponse GetEmailContent([ActionParameter] GetEmailInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}/content.json", Method.Get, Credentials);
        var response = Client.ExecuteWithError<EmailContentDto>(request);
        return new(response.Result);
    }


    [Action("Update email content", Description = "Update content of a specific email")]
    public void UpdateEmailContent(
        [ActionParameter] GetEmailInfoRequest emailRequest,
        [ActionParameter] UpdateContentRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{emailRequest.EmailId}/content.json", Method.Post,
                Credentials)
            .AddParameter("type", input.Type)
            .AddParameter("content", input.Content);

        Client.ExecuteWithErrorHandling(request);
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

        var emailContent = $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content.json";
        var emailContentRequest = new MarketoRequest(emailContent, Method.Get, Credentials).AddQueryParameter("maxReturn", 200);
        var emailContentResponse = Client.ExecuteWithError<EmailContentDto>(emailContentRequest);
        var sectionContent = emailContentResponse.Result!
            .Where(x => x.ContentType == "DynamicContent" || x.ContentType == "Text")
            .ToDictionary(
                x => x.HtmlId, 
                y => GetEmailSectionContent(getEmailInfoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, y));
        var resultHtml = GenerateHtml(sectionContent, emailInfo.Name, getSegmentBySegmentationRequest.Segment);
        
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
        var emailContentFirstResponse = GetEmailContent(getEmailInfoRequest);

        if (!translateEmailWithHtmlRequest.TranslateOnlyDynamic)
        {
            foreach (var item in emailContentFirstResponse.EmailContentItems)
            {
                if (item.ContentType == "Text")
                {
                    ConvertSectionToDynamicContent(getEmailInfoRequest.EmailId, item.HtmlId, getSegmentationRequest.SegmentationId);
                }
            }
        }

        var emailContentSecondResponse = GetEmailContent(getEmailInfoRequest);
        var translatedContent = ParseHtml(translateEmailWithHtmlRequest.File);
        foreach (var item in emailContentSecondResponse.EmailContentItems)
        {
            if(item.ContentType == "DynamicContent")
            {
                UpdateEmailDynamicContent(getEmailInfoRequest, getSegmentBySegmentationRequest, item.Value.ToString(), translatedContent[item.HtmlId]);
            }
        }
    }

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

    private IdDto ConvertSectionToDynamicContent(string emailId, string htmlId, string segmentationId)
    {
        var endpoint = $"/rest/asset/v1/email/{emailId}/content/{htmlId}.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddParameter("value", segmentationId)
            .AddParameter("type", "DynamicContent");
        return Client.ExecuteWithError<IdDto>(request).Result.FirstOrDefault();
    }

    private IdDto UpdateEmailDynamicContent(
        GetEmailInfoRequest getEmailInfoRequest,
        GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        string dynamicContentId,
        string content)
    {
        var endpoint =
            $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{dynamicContentId}.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddQueryParameter("segment", getSegmentBySegmentationRequest.Segment)
            .AddQueryParameter("type", "HTML")
            .AddQueryParameter("value", content);

        return Client.GetSingleEntity<IdDto>(request);
    }

    private string GetEmailSectionContent(
        GetEmailInfoRequest getEmailInfoRequest,
        GetSegmentationRequest getSegmentationRequest,
        GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        EmailContentDto sectionContent)
    {
        if(sectionContent.ContentType == "DynamicContent")
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

    private string GenerateHtml(Dictionary<string, string> sections,
        string title, string language)
    {
        var htmlDoc = new HtmlDocument();
        var htmlNode = htmlDoc.CreateElement("html");
        htmlDoc.DocumentNode.AppendChild(htmlNode);

        var headNode = htmlDoc.CreateElement("head");
        htmlNode.AppendChild(headNode);

        var titleNode = htmlDoc.CreateElement("title");
        headNode.AppendChild(titleNode);
        titleNode.InnerHtml = title;

        var bodyNode = htmlDoc.CreateElement("body");
        bodyNode.SetAttributeValue(LanguageAttribute, language);
        htmlNode.AppendChild(bodyNode);

        foreach(var section in sections)
        {
            var sectionNode = htmlDoc.CreateElement("div");
            sectionNode.SetAttributeValue(HtmlIdAttribute, section.Key);
            sectionNode.InnerHtml = section.Value;
            bodyNode.AppendChild(sectionNode);
        }
        return htmlDoc.DocumentNode.OuterHtml;
    }

    private Dictionary<string, string> ParseHtml(FileReference file)
    {
        var result = new Dictionary<string, string>();

        var formBytes = _fileManagementClient.DownloadAsync(file).Result.GetByteData().Result;
        var html = Encoding.UTF8.GetString(formBytes);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
        var sections = htmlDoc.DocumentNode.SelectSingleNode("//body").ChildNodes;
        foreach(var section in sections)
        {
            result.Add(section.Attributes[HtmlIdAttribute].Value, section.InnerHtml);
        }
        return result;
    }
}