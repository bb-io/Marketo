using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Webhooks.Payload
{
    public class EventPayload
    {
        [Display("Field value 1")]
        public string? Field1 { get; set; }

        [Display("Field value 2")]
        public string? Field2 { get; set; }

        [Display("Field value 3")]
        public string? Field3 { get; set; }

        [Display("Field value 4")]
        public string? Field4 { get; set; }

        [Display("Field value 5")]
        public string? Field5 { get; set; }
    }
}
