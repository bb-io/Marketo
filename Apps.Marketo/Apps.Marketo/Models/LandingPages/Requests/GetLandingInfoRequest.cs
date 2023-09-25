using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.LandingPages.Requests
{
    public class GetLandingInfoRequest
    {
        [DataSource(typeof(LandingPageHandler))]
        [Display("Landing page")]
        public string Id { get; set; }
    }
}
