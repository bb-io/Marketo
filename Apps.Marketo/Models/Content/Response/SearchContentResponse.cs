using Apps.Marketo.Dtos.Content;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Marketo.Models.Content.Response;

public record SearchContentResponse(List<ContentDto> Items) : IMultiDownloadableContentOutput<ContentDto>
{
    public List<ContentDto> Items { get; set; } = Items;
};