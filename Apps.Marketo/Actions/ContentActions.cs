using Apps.Marketo.Extensions;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Content.Response;
using Apps.Marketo.Services.Content;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.Marketo.Actions;

[ActionList("Content")]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) :
    MarketoInvocable(invocationContext)
{
    private readonly ContentServiceFactory _factory = new(invocationContext, fileManagementClient);

    [BlueprintActionDefinition(BlueprintAction.SearchContent)]
    [Display("Search content", Description = "Search different content types")]
    public async Task<SearchContentResponse> SearchContent([ActionParameter] SearchContentRequest input)
    {
        input.Validate().ApplyDefaultValues();

        var services = _factory.GetContentServices(input.ContentTypes!);
        return await services.ExecuteManySearch(input);
    }
}
