using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Emails.Requests
{
    public class UpdateEmailDynamicContentRequest
    {
        [Display("New HTML content")]
        public string HTMLContent { get; set; }
    }
}
