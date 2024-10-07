using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Emails.Requests
{
    public class UpdateEmailMetadataRequest
    {
        [Display("Name")]
        public string? Name { get; set; }

        [Display("Description")]
        public string? Description { get; set; }
    }
}
