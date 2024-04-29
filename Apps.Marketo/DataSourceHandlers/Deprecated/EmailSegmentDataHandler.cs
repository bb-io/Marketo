using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Emails.Requests;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers.Deprecated
{
    public class EmailSegmentDataHandler : BaseInvocable, IAsyncDataSourceHandler
    {
        public GetEmailInfoRequest EmailInfoRequest { get; set; }
        public GetEmailDynamicItemRequest DynamicItemRequest { get; set; }
        public EmailSegmentDataHandler(InvocationContext invocationContext,
            [ActionParameter] GetEmailInfoRequest emailInfoRequest,
            [ActionParameter] GetEmailDynamicItemRequest dynamicItemRequest) : base(invocationContext)
        {
            EmailInfoRequest = emailInfoRequest;
            DynamicItemRequest = dynamicItemRequest;
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            if (EmailInfoRequest == null)
                throw new ArgumentException("Please, specify an email first");
            if (DynamicItemRequest == null)
                throw new ArgumentException("Please, specify an email dynamic content first");

            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/email/{EmailInfoRequest.EmailId}/dynamicContent/{DynamicItemRequest.DynamicContentId}.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var response = client.Paginate<DynamicContentDto>(request);

            return response.First().Content.Where(s => s.Type == "HTML").ToDictionary(k => k.SegmentName, v => v.SegmentName);
        }
    }
}
