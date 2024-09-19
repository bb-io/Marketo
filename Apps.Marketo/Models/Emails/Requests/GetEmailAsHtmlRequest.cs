using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Emails.Requests
{
    public class GetEmailAsHtmlRequest
    {
        [Display("Get only dynamic content")]
        public bool? GetOnlyDynamicContent { get; set; }

        [Display("Include images")]
        public bool? IncludeImages { get; set; }
    }
}
