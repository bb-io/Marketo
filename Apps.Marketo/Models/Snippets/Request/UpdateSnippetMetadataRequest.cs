using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Snippets.Request
{
    public class UpdateSnippetMetadataRequest
    {
        [Display("Name")]
        public string? Name { get; set; }

        [Display("Description")]
        public string? Description { get; set; }
    }
}
