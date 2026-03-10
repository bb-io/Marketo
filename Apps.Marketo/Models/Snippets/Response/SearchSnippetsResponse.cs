using Apps.Marketo.Dtos.Snippet;

namespace Apps.Marketo.Models.Snippets.Response;

public record SearchSnippetsResponse(List<SnippetDto> Snippets);