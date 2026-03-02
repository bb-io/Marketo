using Apps.Marketo.Dtos.Content;
using Apps.Marketo.Extensions;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Content.Response;
using Apps.Marketo.Models.Entities.Snippet;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;

namespace Apps.Marketo.Services.Content.Concrete;

public class SnippetContentService(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext), IContentService
{
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
}
