using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.LandingPages.Requests
{
    public class UpdateLandingMetadataRequest
    {
        [Display("Name")]
        public string? Name { get; set; }

        [Display("Description")]
        public string? Description { get; set; }
    }
}
