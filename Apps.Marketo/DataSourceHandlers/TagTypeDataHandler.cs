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
    public class TagTypeDataHandler : BaseInvocable, IAsyncDataSourceHandler
    {
        public TagTypeDataHandler(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);

            var request = new MarketoRequest("/rest/asset/v1/tagTypes.json", Method.Get,
                InvocationContext.AuthenticationCredentialsProviders);
            var items = client.ExecuteWithError<TagTypeDto>(request);

            return items.Result
                .Where(str =>
                    context.SearchString is null ||
                    str.TagType.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                .Take(50)
                .ToDictionary(k => k.TagType, v => v.TagType);
        }
    }
}
