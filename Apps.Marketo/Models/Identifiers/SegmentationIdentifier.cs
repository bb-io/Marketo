using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Identifiers;

public class SegmentationIdentifier
{
    [Display("Segmentation ID"), DataSource(typeof(SegmentationDataHandler))]
    public string SegmentationId { get; set; }
}
