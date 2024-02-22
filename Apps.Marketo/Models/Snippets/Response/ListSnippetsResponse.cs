using Apps.Marketo.Dtos;

namespace Apps.Marketo.Models.Snippets.Response;

public class ListSnippetsResponse
{
    public IEnumerable<SnippetDto> Snippets { get; set; }
}