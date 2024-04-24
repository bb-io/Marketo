using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Tags.Request;
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
    public class TagValueDataHandler : BaseInvocable, IAsyncDataSourceHandler
    {
        public GetTagTypeRequest GetTagTypeRequest {  get; set; }
        public TagValueDataHandler(InvocationContext invocationContext, [ActionParameter] GetTagTypeRequest getTagTypeRequest) : base(invocationContext)
        {
            GetTagTypeRequest = getTagTypeRequest;
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            if(string.IsNullOrEmpty(GetTagTypeRequest.TagType))
                throw new ArgumentException("Fill tag type first!");

            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var request = new MarketoRequest("/rest/asset/v1/tagType/byName.json", Method.Get,
                InvocationContext.AuthenticationCredentialsProviders);
            request.AddQueryParameter("name", GetTagTypeRequest.TagType);
            var items = client.Paginate<TagValueDto>(request);
            var tagValues = items.SelectMany(x => x.AllowableValues.Substring(1, x.AllowableValues.Length - 2).Split(", ")).ToList();
            return tagValues
                .Where(str =>
                    context.SearchString is null ||
                    str.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                .Take(50)
                .ToDictionary(k => k, v => v);
        }
    }
}
