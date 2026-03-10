using Apps.Marketo.Dtos;

namespace Apps.Marketo.Models.Entities.Snippet;

public class SnippetDynamicContentEntity
{
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
    public string Id { get; set; }
    public string Segmentation { get; set; }
    public List<SnippetSegmentEntity> Content { get; set; }
}
