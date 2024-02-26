using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Emails.Requests
{
    public class GetSegmentBySegmentationRequest
    {
        [Display("Segment")]
        [DataSource(typeof(SegmentBySegmentationDataHandler))]
        public string Segment { get; set; }
    }
}
