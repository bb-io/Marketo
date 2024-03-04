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

namespace Apps.Marketo.DataSourceHandlers.FolderDataHandlers
{
    public class SnippetFolderDataHandler : BaseInvocable, IAsyncDataSourceHandler
    {
        public SnippetFolderDataHandler(InvocationContext invocationContext) : base(invocationContext)
        {
        }
        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest($"/rest/asset/v1/folders.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            request.AddQueryParameter("maxDepth", 10);
            var response = client.Paginate<FolderInfoDto>(request);
            return response
                .DistinctBy(x => x.Id)
                .Where(x => x.FolderType == "Snippet")
                .Where(str => str.Name.Contains(context.SearchString))
                .ToDictionary(k => k.Id.ToString(), v => v.Name);
        }
    }
}
