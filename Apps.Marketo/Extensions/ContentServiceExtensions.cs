using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Content.Response;
using Apps.Marketo.Services.Content;

namespace Apps.Marketo.Extensions;

public static class ContentServiceExtensions
{
    public static async Task<SearchContentResponse> ExecuteManySearch(
        this List<IContentService> contentServices,
        SearchContentRequest request)
    {
        var searchTasks = contentServices.Select(service => service.SearchContent(request));
        var results = await Task.WhenAll(searchTasks);

        var allItems = results.SelectMany(x => x.Items).ToList();
        return new(allItems);
    }
}