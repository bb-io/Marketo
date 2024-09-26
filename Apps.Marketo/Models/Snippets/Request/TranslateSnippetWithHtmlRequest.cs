using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Marketo.Models.Snippets.Request
{
    public class TranslateSnippetWithHtmlRequest
    {
        [Display("Translated HTML file")]
        public FileReference File { get; set; }
    }
}
