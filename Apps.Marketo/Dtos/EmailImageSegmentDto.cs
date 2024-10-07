namespace Apps.Marketo.Dtos;
public class EmailImageSegmentDto : EmailBaseSegmentDto
{
    public string ContentUrl { get; set; }
    public string Height { get; set; }
    public string Width { get; set; }
    public string AltText { get; set; }
    public string Style { get; set; }
}

