namespace Apps.Marketo.Dtos;
public class LandingPageImageSegmentDto<T>
{
    public int SegmentId { get; set; }
    public string SegmentName { get; set; }
    public string Type { get; set; }
    public T Content { get; set; }
    public FormattingOptions FormattingOptions { get; set; }
}

public class LandingPageImageContent
{
    public string Content { get; set; }
    public string ContentType { get; set; }
    public string ContentUrl { get; set; }
}

public class FormattingOptions
{
    public string Height { get; set; }
    public string Width { get; set; }
    public int ZIndex { get; set; }
    public string Left { get; set; }
    public string Top { get; set; }
    public bool ImageOpenNewWindow { get; set; }
}