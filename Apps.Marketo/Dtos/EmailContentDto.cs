namespace Apps.Marketo.Dtos;

public class EmailContentDto
{
    public string ContentType { get; set; }
    public string HtmlId { get; set; }
    public int Index { get; set; }
    public bool IsLocked { get; set; }
    public string ParentHtmlId { get; set; }
    public IEnumerable<EmailContentValueDto> Value { get; set; }
}