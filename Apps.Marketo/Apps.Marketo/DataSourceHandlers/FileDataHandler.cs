using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.Marketo.Dtos;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers
{
    public class FileDataHandler : BaseInvocable, IAsyncDataSourceHandler
    {
        public FileDataHandler(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/files.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var response = client.Execute<FetchFileDto>(request);
            return response.Data.Result.ToDictionary(k => k.Id.ToString(), v => v.Name);
        }
    }
}
