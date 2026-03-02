using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Emails.Requests;

public class DownloadEmailRequest
{
    [Display("Get only dynamic content")]
    public bool? GetOnlyDynamicContent { get; set; }

    [Display("Include images")]
    public bool? IncludeImages { get; set; }

    [Display("Segmentation ID"), DataSource(typeof(SegmentationDataHandler))]
    public string? SegmentationId { get; set; }

    [Display("Segment"), DataSource(typeof(SegmentBySegmentationDataHandler))]
    public string? Segment { get; set; }
}
