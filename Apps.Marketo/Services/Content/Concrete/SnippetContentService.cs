using Apps.Marketo.Constants;
using Apps.Marketo.Dtos;
using Apps.Marketo.Dtos.Content;
using Apps.Marketo.Extensions;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.HtmlHelpers;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Content.Response;
using Apps.Marketo.Models.Entities;
using Apps.Marketo.Models.Entities.Snippet;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;
using System.Net.Mime;
using System.Text;

namespace Apps.Marketo.Services.Content.Concrete;

public class SnippetContentService(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext), IContentService
{
    public async Task<DownloadContentResponse> DownloadContent(DownloadContentRequest input)
    {
        var snippetInfoRequest = new RestRequest($"/rest/asset/v1/snippet/{input.ContentId}.json", Method.Get);
        var snippetInfo = await Client.ExecuteWithErrorHandlingFirst<SnippetEntity>(snippetInfoRequest);

        var snippetContentRequest = new RestRequest($"/rest/asset/v1/snippet/{input.ContentId}/content.json", Method.Get);
        var snippetContent = await Client.ExecuteWithErrorHandling<SnippetContentDto>(snippetContentRequest);

        if (snippetContent.Count() == 1 && snippetContent.First().Type == "DynamicContent")
        {
            snippetContent =
                (await GetSnippetDynamicContent(input.ContentId, input.SegmentationId, input.Segment))
                .Select(x => new SnippetContentDto(x.Type, x.Content)).ToList();
        }
        var sectionContent = snippetContent
            .ToDictionary(
                x => x.Type,
                y => y.Content);
        var resultHtml = HtmlContentBuilder.GenerateHtml(
            sectionContent,
            snippetInfo.Name,
            input.Segment ?? string.Empty,
            new HtmlIdEntity(MetadataConstants.BlackbirdSnippetId, input.ContentId));

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        var file = await fileManagementClient.UploadAsync(stream, MediaTypeNames.Text.Html, snippetInfo.Name.ToHtmlFileName());
        return new(file);
    }

    public async Task<SearchContentResponse> SearchContent(SearchContentRequest input)
    {
        var request = new RestRequest("/rest/asset/v1/snippets.json", Method.Get);
        request.AddQueryParameterIfNotNull("status", input.Status);

        var snippets = await Client.Paginate<SnippetEntity>(request);

        snippets = snippets
            .ApplyDateAfterFilter(input.CreatedAfter, x => x.CreatedAt)
            .ApplyDateBeforeFilter(input.CreatedBefore, x => x.CreatedAt)
            .ApplyDateAfterFilter(input.UpdatedAfter, x => x.UpdatedAt)
            .ApplyDateBeforeFilter(input.UpdatedBefore, x => x.UpdatedAt)
            .ApplyNamePatternFilter(input.NamePatterns, input.ExcludeMatched, x => x.Name)
            .ApplyFolderIdFilter(input.FolderId, x => x.Folder);

        snippets = await snippets.ApplyIgnoreInArchiveFilter(Client, input.IgnoreInArchive, x => x.Folder);

        return new(snippets.Select(x => new ContentDto(x)).ToList());
    }

    private async Task<List<SnippetSegmentEntity>> GetSnippetDynamicContent(
        string snippetId,
        string? segmentationId,
        string? segmentName)
    {
        var requestSeg = new RestRequest($"/rest/asset/v1/snippet/{snippetId}/dynamicContent.json", Method.Get);
        var responseSeg = await Client.ExecuteWithErrorHandlingFirst<SnippetDynamicContentEntity>(requestSeg);

        if (!string.IsNullOrEmpty(segmentationId) && responseSeg.Segmentation != segmentationId)
        {
            throw new PluginMisconfigurationException(
                $"Segmentation mismatch! You selected segmentation ID {segmentationId}, " +
                $"but this snippet is segmented by ID {responseSeg.Segmentation}."
            );
        }

        var targetSegmentName = string.IsNullOrWhiteSpace(segmentName) ? "Default" : segmentName;
        var targetSegment = responseSeg.Content.FirstOrDefault(x => x.SegmentName == targetSegmentName)
                         ?? responseSeg.Content.FirstOrDefault(x => x.SegmentName == "Default");

        if (targetSegment == null)
            return [];

        return [targetSegment];
    }
}
