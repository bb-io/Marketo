using Apps.Marketo.Dtos;

namespace Apps.Marketo.Models.Snippets.Response;

public record SearchSnippetsResponse(List<SnippetDto> Snippets);