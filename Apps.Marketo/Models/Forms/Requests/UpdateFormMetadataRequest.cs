using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Forms.Requests
{
    public class UpdateFormMetadataRequest
    {
        [Display("Name")]
        public string? Name { get; set; }

        [Display("Description")]
        public string? Description { get; set; }
    }
}
