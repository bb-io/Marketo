using Apps.Marketo.Constants;
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
using Blackbird.Applications.Sdk.Common.Exceptions;
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
    [Action("Search content", Description = "Search for content across multiple content types using specific criteria")]
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
    [Action("Download content", Description = "Download content of a specific content type")]
    public async Task<DownloadContentResponse> DownloadContent(
        [ActionParameter] ContentTypeIdentifier contentType,
        [ActionParameter] DownloadContentRequest input)
    {
        if (IsSegmentableContentType(contentType.ContentType) && 
            (string.IsNullOrEmpty(input.Segment) || string.IsNullOrEmpty(input.SegmentationId)))
        {
            throw new PluginMisconfigurationException(
                "Both 'Segment' and 'Segmentation ID' inputs " +
                "are required to select when downloading emails, landing pages or snippets");
        }

        var service = _factory.GetContentService(contentType.ContentType);
        var file = await service.DownloadContent(input);
        return new(file);
    }

    [BlueprintActionDefinition(BlueprintAction.UploadContent)]
    [Action("Upload content", Description = "Upload content of a specific content type")]
    public async Task UploadContent([ActionParameter] UploadContentRequest uploadInput)
    {
        string html = await ContentDownloader.DownloadHtmlContent(fileManagementClient, uploadInput.Content);
        var input = new UploadContentInput(html, uploadInput);

        string contentType = uploadInput.ContentType ?? ContentDetector.DetectContentType(html);
        var service = _factory.GetContentService(contentType);
        await service.UploadContent(input);
    }

    private static bool IsSegmentableContentType(string contentType)
    {
        return 
            contentType == ContentTypes.Email ||
            contentType == ContentTypes.Snippet ||
            contentType == ContentTypes.LandingPage;
    }
}
