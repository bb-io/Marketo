using Apps.Marketo.Dtos.Content;
using Apps.Marketo.Extensions;
using Apps.Marketo.Helper.FileFolder;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Content.Response;
using Apps.Marketo.Models.Entities.LandingPage;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;

namespace Apps.Marketo.Services.Content.Concrete;

public class LandingPageContentService(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext), IContentService
{
    public async Task<SearchContentResponse> SearchContent(SearchContentRequest input)
    {
        var request = new RestRequest($"/rest/asset/v1/landingPages.json", Method.Get);
        await FileFolderHelper.AddFolderParameter(Client, request, input.FolderId);
        request.AddQueryParameterIfNotNull("status", input.Status);

        var pages = await Client.Paginate<LandingPageEntity>(request);

        pages = pages
            .ApplyDateAfterFilter(input.CreatedAfter, x => x.CreatedAt)
            .ApplyDateBeforeFilter(input.CreatedBefore, x => x.CreatedAt)
            .ApplyDateAfterFilter(input.UpdatedAfter, x => x.UpdatedAt)
            .ApplyDateBeforeFilter(input.UpdatedBefore, x => x.UpdatedAt)
            .ApplyNamePatternFilter(input.NamePatterns, input.ExcludeMatched, x => x.Name);

        pages = await pages.ApplyIgnoreInArchiveFilter(Client, input.IgnoreInArchive, x => x.Folder);

        return new(pages.Select(x => new ContentDto(x)).ToList());
    }
}
