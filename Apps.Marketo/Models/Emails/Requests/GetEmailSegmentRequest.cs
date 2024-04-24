using Apps.Marketo.DataSourceHandlers.Deprecated;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Emails.Requests
{
    public class GetEmailSegmentRequest
    {
        [Display("Segment")]
        [DataSource(typeof(EmailSegmentDataHandler))]
        public string Segment { get; set; }
    }
}
