using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Marketo.Models.Content.Response;

public class ContentResponse : IDownloadContentInput
{
    [Display("Content ID")]
    public string ContentId { get; set; } = string.Empty;

    [Display("Content name")]
    public string ContentName { get; set; } = string.Empty;

    [Display("Content type")]
    public string ContentType { get; set; } = string.Empty;

    [Display("Content description")]
    public string ContentDescription { get; set; } = string.Empty;

    [Display("Content URL")]
    public string ContentUrl { get; set; } = string.Empty;

    [Display("Created at")]
    public DateTime CreatedAt { get; set; }

    [Display("Updated at")]
    public DateTime UpdatedAt { get; set; }

    [Display("Status")]
    public string Status { get; set; } = string.Empty;
}
