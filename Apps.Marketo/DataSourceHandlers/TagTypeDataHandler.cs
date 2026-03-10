using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Marketo.DataSourceHandlers
{
    public class TagTypeDataHandler : MarketoInvocable, IAsyncDataSourceHandler
    {
        public TagTypeDataHandler(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var request = new RestRequest("/rest/asset/v1/tagTypes.json", Method.Get);
            var items = await Client.ExecuteWithErrorHandling<TagTypeDto>(request);

            return items
                .Where(str =>
                    context.SearchString is null ||
                    str.TagType.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                .Take(50)
                .ToDictionary(k => k.TagType, v => v.TagType);
        }
    }
}
