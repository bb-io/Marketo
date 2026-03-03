using Apps.Marketo.Extensions;
using Apps.Marketo.Helper.Content;
using Apps.Marketo.Helper.FileFolder;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Content.Response;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.Identifiers.Optional;
using Apps.Marketo.Services.Content;
using Apps.Marketo.Services.Content.Models;
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
    public async Task<SearchContentResponse> SearchContent(
        [ActionParameter] OptionalContentTypesIdentifier contentTypesInput,
        [ActionParameter] SearchContentRequest input)
    {
        input.Validate();
        contentTypesInput.ApplyDefaultValues();

        var services = _factory.GetContentServices(contentTypesInput.ContentTypes!);
        return await services.ExecuteManySearch(input);
    }

    [BlueprintActionDefinition(BlueprintAction.DownloadContent)]
    [Display("Download content", Description = "Download content")]
    public async Task<DownloadContentResponse> DownloadContent(
        [ActionParameter] ContentTypeIdentifier contentType,
        [ActionParameter] DownloadContentRequest input)
    {
        var service = _factory.GetContentService(contentType.ContentType);
        var file = await service.DownloadContent(input);
        return new(file);
    }

    [BlueprintActionDefinition(BlueprintAction.UploadContent)]
    [Display("Upload content", Description = "Upload content")]
    public async Task UploadContent([ActionParameter] UploadContentRequest uploadInput)
    {
        string html = await ContentDownloader.DownloadHtmlContent(fileManagementClient, uploadInput.Content);
        var input = new UploadContentInput(html, uploadInput);

        string contentType = uploadInput.ContentType ?? ContentDetector.DetectContentType(html);
        var service = _factory.GetContentService(contentType);
        await service.UploadContent(input);
    }
}
