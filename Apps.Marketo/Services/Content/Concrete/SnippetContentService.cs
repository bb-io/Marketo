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
using Apps.Marketo.Services.Content.Models;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;
using System.Net.Mime;
using System.Text;

namespace Apps.Marketo.Services.Content.Concrete;

public class SnippetContentService(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext), IContentService
{
    public async Task UploadContent(UploadContentInput input)
    {
        var metaTags = HtmlContentBuilder.ExtractAllMetaTags(input.HtmlContent);

        string snippetId = HtmlContentBuilder.GetRequiredMetaValue(
            input.ContentId, metaTags, MetadataConstants.BlackbirdSnippetId, "Snippet ID");

        string segmentationId = HtmlContentBuilder.GetRequiredMetaValue(
            input.SegmentationId, metaTags, MetadataConstants.BlackbirdSegmentationId, "Segmentation ID");

        string segment = HtmlContentBuilder.GetRequiredMetaValue(
            input.Segment, metaTags, MetadataConstants.BlackbirdSegmentName, "Segment name");

        var snippetContentRequest = new RestRequest($"/rest/asset/v1/snippet/{snippetId}/content.json", Method.Get);
        var snippetContentResponse = await Client.ExecuteWithErrorHandling<SnippetContentDto>(snippetContentRequest);

        if (!(snippetContentResponse.Count() == 1 && snippetContentResponse.First().Type == "DynamicContent"))
        {
            await ConvertSnippetToDynamicContent(snippetId, segmentationId);
        }
        var snippetDynamicContent = await GetSnippetDynamicContent(snippetId, segmentationId, segment);

        var translatedContent = HtmlContentBuilder.ParseHtml(input.HtmlContent);
        foreach (var item in snippetDynamicContent.Where(x => x.SegmentName == segment).ToList())
        {
            if ((item.Type == "HTML" || item.Type == "Text") &&
                translatedContent.TryGetValue(item.Type, out var translatedContentItem))
            {
                await UpdateSnippetDynamicContent(snippetId, item.SegmentId.ToString(), item.Type, translatedContentItem);
            }
        }
    }

    private async Task<IdDto> ConvertSnippetToDynamicContent(string snippetId, string segmentationId)
    {
        var endpoint = $"/rest/asset/v1/snippet/{snippetId}/content.json";
        var request = new RestRequest(endpoint, Method.Post)
            .AddParameter("content", segmentationId)
            .AddParameter("type", "DynamicContent");
        return await Client.ExecuteWithErrorHandlingFirst<IdDto>(request);
    }

    private async Task<IdDto> UpdateSnippetDynamicContent(
        string snippetId,
        string segmentId,
        string contentType,
        string content)
    {
        var request = new RestRequest($"/rest/asset/v1/snippet/{snippetId}/dynamicContent/{segmentId}.json", Method.Post)
            .AddQueryParameter("type", contentType)
            .AddQueryParameter("value", content);

        return await Client.ExecuteWithErrorHandlingFirst<IdDto>(request);
    }

    public async Task<FileReference> DownloadContent(DownloadContentRequest input)
    {
        var snippetInfoRequest = new RestRequest($"/rest/asset/v1/snippet/{input.ContentId}.json", Method.Get);
        var snippetInfo = await Client.ExecuteWithErrorHandlingFirst<SnippetEntity>(snippetInfoRequest);

        var snippetContentRequest = new RestRequest($"/rest/asset/v1/snippet/{input.ContentId}/content.json", Method.Get);
        var snippetContent = await Client.ExecuteWithErrorHandling<SnippetContentDto>(snippetContentRequest);

        var snippetContentList = snippetContent.ToList();

        var metadata = new List<MetadataEntity> { new(MetadataConstants.BlackbirdSnippetId, input.ContentId) };

        if (snippetContentList.Count == 1 && snippetContentList[0].Type == "DynamicContent")
        {
            var dynamicSegments = await GetSnippetDynamicContent(input.ContentId, input.SegmentationId, input.Segment);
            snippetContentList = dynamicSegments.Select(x => new SnippetContentDto(x.Type, x.Content)).ToList();

            var resolvedSegmentName = string.IsNullOrWhiteSpace(input.Segment) ? "Default" : input.Segment;
            metadata.Add(new(MetadataConstants.BlackbirdSegmentName, resolvedSegmentName));

            if (!string.IsNullOrWhiteSpace(input.SegmentationId))
                metadata.Add(new(MetadataConstants.BlackbirdSegmentationId, input.SegmentationId));
        }

        var sectionContent = snippetContentList
            .GroupBy(x => x.Type)
            .ToDictionary(
                g => g.Key,
                g => string.Join(string.Empty, g.Select(y => y.Content))
            );

        var resultHtml = HtmlContentBuilder.GenerateHtml(sectionContent, snippetInfo.Name, metadata);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        return await fileManagementClient.UploadAsync(stream, MediaTypeNames.Text.Html, snippetInfo.Name.ToHtmlFileName());
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
