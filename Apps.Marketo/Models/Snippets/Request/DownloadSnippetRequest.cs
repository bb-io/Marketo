using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Snippets.Request;

public class DownloadSnippetRequest
{
    [Display("Segmentation ID"), DataSource(typeof(SegmentationDataHandler))]
    public string? SegmentationId { get; set; }

    [Display("Segment"), DataSource(typeof(SegmentBySegmentationDataHandler))]
    public string? Segment { get; set; }
}
