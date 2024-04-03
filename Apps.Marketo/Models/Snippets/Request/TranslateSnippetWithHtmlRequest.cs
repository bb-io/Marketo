using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Snippets.Request
{
    public class TranslateSnippetWithHtmlRequest
    {
        [Display("Translated HTML file")]
        public FileReference File { get; set; }
    }
}
