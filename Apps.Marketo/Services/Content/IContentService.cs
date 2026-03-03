using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Content.Response;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Marketo.Services.Content;

public interface IContentService
{
    Task<SearchContentResponse> SearchContent(SearchContentRequest input);
    Task<FileReference> DownloadContent(DownloadContentRequest input);
}
