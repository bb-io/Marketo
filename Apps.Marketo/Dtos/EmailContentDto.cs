using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Dtos;

public class EmailContentDto
{
    [Display("Content type")]
    public string ContentType { get; set; }

    [Display("HTML ID")]
    public string HtmlId { get; set; }
    public int Index { get; set; }

    [Display("Is locked")]
    public bool IsLocked { get; set; }

    [Display("Parent HTML ID")]
    public string ParentHtmlId { get; set; }
    public object Value { get; set; }
}