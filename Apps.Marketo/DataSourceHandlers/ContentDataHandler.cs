using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Services.Content;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Marketo.DataSourceHandlers;

public class ContentDataHandler(
    InvocationContext invocationContext,
    [ActionParameter] ContentTypeIdentifier contentType)
    : MarketoInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    private readonly ContentServiceFactory _factory = new(invocationContext, null!);

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(contentType.ContentType))
            throw new PluginMisconfigurationException("Please specify content type first");

        var service = _factory.GetContentService(contentType.ContentType);

        var input = new SearchContentRequest { NamePatterns = [$"*{context.SearchString}*"] };
        var result = await service.SearchContent(input);

        return result.Items.Select(x => new DataSourceItem(x.ContentId, x.ContentName)).ToList();
    }
}
