using Apps.Marketo.Dtos;

namespace Apps.Marketo.Models.Snippets.Response;

public record ListSnippetsResponse(List<SnippetDto> Snippets);