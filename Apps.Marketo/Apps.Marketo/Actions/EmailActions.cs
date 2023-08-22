using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Emails.Requests;
using Apps.Marketo.Models.Emails.Responses;
using Apps.Marketo.Models.Files.Requests;
using Apps.Marketo.Models.Files.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Actions
{
    [ActionList]
    public class EmailActions : BaseInvocable
    {
        public EmailActions(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        [Action("List all emails", Description = "List all emails")]
        public ListEmailsResponse ListEmails([ActionParameter] ListEmailsRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/emails.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            if(input.Status != null) request.AddQueryParameter("status", input.Status);
            if(input.FolderId != null) request.AddQueryParameter("folder", JsonConvert.SerializeObject(new { id = int.Parse(input.FolderId), type = input.Type ?? "Folder"}));
            request.AddQueryParameter("offset", input.Offset ?? 0);
            request.AddQueryParameter("maxReturn", input.MaxReturn ?? 200);
            if (input.EarliestUpdatedAt != null) request.AddQueryParameter("earliestUpdatedAt", ((DateTime)input.EarliestUpdatedAt).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
            if (input.LatestUpdatedAt != null) request.AddQueryParameter("latestUpdatedAt", ((DateTime)input.LatestUpdatedAt).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));

            var response = client.Execute<BaseResponseDto<EmailDto>>(request).Data;
            return new ListEmailsResponse() { Emails = response.Result };
        }

        [Action("Get email info", Description = "Get email info")]
        public EmailDto GetEmailInfo([ActionParameter] GetEmailInfoRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var response = client.Execute<BaseResponseDto<EmailDto>>(request);
            return response.Data.Result.First();
        }

        [Action("Get email content", Description = "Get email content")]
        public EmailContentResponse GetEmailContent([ActionParameter] GetEmailInfoRequest input)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}/content.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var response = client.Execute<BaseResponseDto<EmailContentDto>>(request);    
            return new EmailContentResponse(response.Data.Result.First());
        }
    }
}
