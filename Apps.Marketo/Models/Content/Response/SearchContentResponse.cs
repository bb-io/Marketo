using Apps.Marketo.Dtos.Content;

namespace Apps.Marketo.Models.Content.Response;

public record SearchContentResponse(List<ContentDto> Items);