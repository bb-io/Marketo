using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Emails.Requests;
using Apps.Marketo.Models.Emails.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;
using System.Globalization;

namespace Apps.Marketo.Actions;

[ActionList]
public class EmailActions : BaseActions
{
    public EmailActions(InvocationContext invocationContext) : base(invocationContext) { }

    [Action("List all emails", Description = "List all emails")]
    public ListEmailsResponse ListEmails([ActionParameter] ListEmailsRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/emails.json", Method.Get, Credentials);
        if(input.Status != null) request.AddQueryParameter("status", input.Status);
        if(input.FolderId != null) request.AddQueryParameter("folder", JsonConvert.SerializeObject(new { id = int.Parse(input.FolderId), type = input.Type ?? "Folder"}));
        request.AddQueryParameter("offset", input.Offset ?? 0);
        request.AddQueryParameter("maxReturn", input.MaxReturn ?? 200);
        if (input.EarliestUpdatedAt != null) request.AddQueryParameter("earliestUpdatedAt", ((DateTime)input.EarliestUpdatedAt).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        if (input.LatestUpdatedAt != null) request.AddQueryParameter("latestUpdatedAt", ((DateTime)input.LatestUpdatedAt).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));

        var response = Client.ExecuteWithError<EmailDto>(request);
        return new ListEmailsResponse() { Emails = response.Result };
    }

    [Action("Get email info", Description = "Get email info")]
    public EmailDto GetEmailInfo([ActionParameter] GetEmailInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}.json", Method.Get, Credentials);
        var response = Client.ExecuteWithError<EmailDto>(request);
        return response.Result.First();
    }

    [Action("Get email content", Description = "Get email content")]
    public EmailContentResponse GetEmailContent([ActionParameter] GetEmailInfoRequest input)
    { 
        var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}/content.json", Method.Get, Credentials);
        var response = Client.ExecuteWithError<EmailContentDto>(request);    
        return new EmailContentResponse(response.Result);
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
        var request = new MarketoRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{getEmailDynamicItemRequest.DynamicContentId}.json", Method.Post, Credentials);
        request.AddQueryParameter("segment", getSegmentBySegmentationRequest.Segment);
        request.AddQueryParameter("type", "HTML");
        request.AddQueryParameter("value", updateEmailDynamicContentRequest.HTMLContent);
        var response = Client.ExecuteWithError<IdDto>(request);
        return response.Result.First();
    }

    [Action("Get email dynamic content", Description = "Get email dynamic content")]
    public GetEmailDynamicContentResponse GetEmailDynamicContent([ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
        [ActionParameter] GetEmailDynamicItemRequest getEmailDynamicItemRequest,
        [ActionParameter] GetEmailSegmentRequest getSegmentRequest)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{getEmailDynamicItemRequest.DynamicContentId}.json", Method.Get, Credentials);
        var response = Client.ExecuteWithError<DynamicContentDto>(request);
        var dynamicContent = response.Result.First().Content.Where(x => x.Type == "HTML" && x.SegmentName == getSegmentRequest.Segment).FirstOrDefault();
        return new GetEmailDynamicContentResponse(dynamicContent) { DynamicContentId = getEmailDynamicItemRequest.DynamicContentId };
    }

    [Action("List email dynamic content", Description = "List email dynamic content by segmentation")]
    public ListEmailDynamicContentResponse ListEmailDynamicContent([ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
        [ActionParameter] GetSegmentationRequest getSegmentationRequest,
        [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest)
    {
        var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
        var request = new MarketoRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
        request.AddQueryParameter("maxReturn", 200);
        var response = client.ExecuteWithError<EmailContentDto>(request);
        var allDynamicContentInfo = response.Result.Where(e => e.ContentType == "DynamicContent").ToList();

        var result = new List<GetEmailDynamicContentResponse>();
        foreach( var dynamicContentInfo in allDynamicContentInfo )
        {
            var requestSeg = new MarketoRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{dynamicContentInfo.Value.ToString()}.json", Method.Get, Credentials);
            var responseSeg = Client.ExecuteWithError<DynamicContentDto>(requestSeg);
            if(responseSeg.Result!.First().Segmentation.ToString() == getSegmentationRequest.SegmentationId)
                result.Add(responseSeg.Result!.First().Content.
                    Where(x => x.Type == "HTML" && x.SegmentName == getSegmentBySegmentationRequest.Segment).
                    Select(x => new GetEmailDynamicContentResponse(x) { DynamicContentId = dynamicContentInfo.Value.ToString()! }).
                    FirstOrDefault()!);
        }
        return new ListEmailDynamicContentResponse() 
        { 
            EmailDynamicContentList = result,
            Segment = getSegmentBySegmentationRequest.Segment
        };
    }
}