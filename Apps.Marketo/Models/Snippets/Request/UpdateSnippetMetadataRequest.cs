using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
