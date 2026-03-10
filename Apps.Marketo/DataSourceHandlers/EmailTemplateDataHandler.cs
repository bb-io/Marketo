using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers
{
    public class EmailTemplateDataHandler : MarketoInvocable, IAsyncDataSourceHandler
    {
        public EmailTemplateDataHandler(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var request = new RestRequest($"/rest/asset/v1/emailTemplates.json", Method.Get);
            var response = await Client.Paginate<EmailTemplateDto>(request);
            return response.Where(str => context.SearchString is null || str.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)).ToDictionary(k => k.Id.ToString(), v => v.Name);
        }
    }
}
