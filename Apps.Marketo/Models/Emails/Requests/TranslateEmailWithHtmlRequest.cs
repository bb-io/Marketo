using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Emails.Requests
{
    public class TranslateEmailWithHtmlRequest
    {
        [Display("Translated HTML file")]
        public FileReference File {  get; set; }

        [Display("Translate only dynamic content")]
        public bool TranslateOnlyDynamic { get; set; }
    }
}
