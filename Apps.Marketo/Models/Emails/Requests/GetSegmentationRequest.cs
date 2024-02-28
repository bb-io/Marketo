using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Emails.Requests
{
    public class GetSegmentationRequest
    {
        [Display("Segmentation ID")]
        [DataSource(typeof(SegmentationDataHandler))]
        public string SegmentationId { get; set; }
    }
}
