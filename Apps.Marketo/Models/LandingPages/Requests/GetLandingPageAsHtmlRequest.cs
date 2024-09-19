using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.LandingPages.Requests
{
    public class GetLandingPageAsHtmlRequest
    {
        [Display("Get only dynamic content")]
        public bool? GetOnlyDynamicContent { get; set; }
    }
}
