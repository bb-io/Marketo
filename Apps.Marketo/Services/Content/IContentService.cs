using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Content.Response;

namespace Apps.Marketo.Services.Content;

public interface IContentService
{
    Task<SearchContentResponse> SearchContent(SearchContentRequest input);
    Task<DownloadContentResponse> DownloadContent(DownloadContentRequest input);
}
