using RestSharp;
using Apps.Marketo.Invocables;
using Apps.Marketo.Extensions;
using Apps.Marketo.Dtos.Content;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.Helper.FileFolder;
using Apps.Marketo.Models.Entities.Email;
using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Content.Response;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.Marketo.Services.Content.Concrete;

public class EmailContentService(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext), IContentService
{
    public async Task<SearchContentResponse> SearchContent(SearchContentRequest input)
    {
        var request = new RestRequest($"/rest/asset/v1/emails.json", Method.Get);
        var subfolders = await FileFolderHelper.AddFolderParameter(Client, request, input.FolderId);
        request
            .AddQueryParameterIfNotNull("status", input.Status)
            .AddQueryParameterIfNotNull("earliestUpdatedAt", input.UpdatedAfter)
            .AddQueryParameterIfNotNull("latestUpdatedAt", input.UpdatedBefore);

        var emails = await Client.Paginate<EmailEntity>(request);

        emails = emails
            .ApplyDateAfterFilter(input.CreatedAfter, x => x.CreatedAt)
            .ApplyDateBeforeFilter(input.CreatedBefore, x => x.CreatedAt)
            .ApplyNamePatternFilter(input.NamePatterns, input.ExcludeMatched, x => x.Name);

        emails = await emails.ApplyIgnoreInArchiveFilter(Client, input.IgnoreInArchive, x => x.Folder);

        return new(emails.Select(x => new ContentDto(x)).ToList());
    }
}
