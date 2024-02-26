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

namespace Apps.Marketo.Actions;

[ActionList]
public class EmailActions : MarketoInvocable
{
    public EmailActions(InvocationContext invocationContext) : base(invocationContext)
    {
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

    [Action("Update email dynamic content", Description = "Update email dynamic content")]
    public IdDto UpdateEmailDynamicContent([ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
        [ActionParameter] GetEmailDynamicItemRequest getEmailDynamicItemRequest,
        [ActionParameter] GetSegmentationRequest getSegmentationRequest,
        [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        [ActionParameter] UpdateEmailDynamicContentRequest updateEmailDynamicContentRequest)
    {
        var endpoint =
            $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{getEmailDynamicItemRequest.DynamicContentId}.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddQueryParameter("segment", getSegmentBySegmentationRequest.Segment)
            .AddQueryParameter("type", "HTML")
            .AddQueryParameter("value", updateEmailDynamicContentRequest.HTMLContent);

        return Client.GetSingleEntity<IdDto>(request);
    }

    [Action("Get email dynamic content", Description = "Get email dynamic content")]
    public GetEmailDynamicContentResponse GetEmailDynamicContent(
        [ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
        [ActionParameter] GetEmailDynamicItemRequest getEmailDynamicItemRequest,
        [ActionParameter] GetEmailSegmentRequest getSegmentRequest)
    {
        var endpoint =
            $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{getEmailDynamicItemRequest.DynamicContentId}.json";
        var request = new MarketoRequest(endpoint, Method.Get, Credentials);
        var response = Client.GetSingleEntity<DynamicContentDto>(request);

        var dynamicContent =
            response.Content.FirstOrDefault(x => x.Type == "HTML" && x.SegmentName == getSegmentRequest.Segment);
        return new(dynamicContent)
        {
            DynamicContentId = getEmailDynamicItemRequest.DynamicContentId
        };
    }

    [Action("List email dynamic content", Description = "List email dynamic content by segmentation")]
    public ListEmailDynamicContentResponse ListEmailDynamicContent(
        [ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
        [ActionParameter] GetSegmentationRequest getSegmentationRequest,
        [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest)
    {
        var endpoint = $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content.json";
        var request = new MarketoRequest(endpoint, Method.Get, Credentials)
            .AddQueryParameter("maxReturn", 200);

        var response = Client.ExecuteWithError<EmailContentDto>(request);
        var allDynamicContentInfo = response.Result.Where(e => e.ContentType == "DynamicContent").ToList();

        var result = new List<GetEmailDynamicContentResponse>();
        foreach (var dynamicContentInfo in allDynamicContentInfo)
        {
            var requestSeg =
                new MarketoRequest(
                    $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{dynamicContentInfo.Value.ToString()}.json",
                    Method.Get, Credentials);
            var responseSeg = Client.ExecuteWithError<DynamicContentDto>(requestSeg);
            if (responseSeg.Result!.First().Segmentation.ToString() == getSegmentationRequest.SegmentationId)
                result.Add(responseSeg.Result!.First().Content
                    .Where(x => x.Type == "HTML" && x.SegmentName == getSegmentBySegmentationRequest.Segment)
                    .Select(x => new GetEmailDynamicContentResponse(x)
                        { DynamicContentId = dynamicContentInfo.Value.ToString()! }).FirstOrDefault()!);
        }
        return new() { EmailDynamicContentList = result };
    }
}