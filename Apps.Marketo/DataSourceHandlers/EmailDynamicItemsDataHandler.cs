using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;
using Apps.Marketo.Models.Emails.Requests;
using System.Net;

namespace Apps.Marketo.DataSourceHandlers
{
    public class EmailDynamicItemsDataHandler : BaseInvocable, IAsyncDataSourceHandler
    {
        public GetEmailInfoRequest EmailInfoRequest { get; set; }
        public EmailDynamicItemsDataHandler(InvocationContext invocationContext, 
            [ActionParameter] GetEmailInfoRequest emailInfoRequest) : base(invocationContext)
        {
            EmailInfoRequest = emailInfoRequest;
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            if (EmailInfoRequest == null)
                throw new ArgumentException("Please, specify an email first");
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/email/{EmailInfoRequest.EmailId}/content.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            request.AddQueryParameter("maxReturn", 200);
            var response = client.ExecuteWithError<EmailContentDto>(request);
            
            return response.Result.Where(e => e.ContentType == "DynamicContent").ToDictionary(k => k.Value.ToString()!, v => v.HtmlId);
        }
    }
}
