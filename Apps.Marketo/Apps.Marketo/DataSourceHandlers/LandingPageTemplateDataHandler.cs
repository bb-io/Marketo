using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.DataSourceHandlers
{
    public class LandingPageTemplateDataHandler : BaseInvocable, IAsyncDataSourceHandler
    {
        public LandingPageTemplateDataHandler(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/landingPageTemplates.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            request.AddQueryParameter("maxReturn", 200);
            var response = client.Execute<BaseResponseDto<LandingPageTemplateDto>>(request);
            return response.Data.Result.Where(str => str.Name.Contains(context.SearchString)).ToDictionary(k => k.Id.ToString(), v => v.Name);
        }
    }
}
