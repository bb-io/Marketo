using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Identifiers;

public class SegmentIdentifier
{
    [Display("Segment"), DataSource(typeof(SegmentBySegmentationDataHandler))]
    public string Segment { get; set; }
}
