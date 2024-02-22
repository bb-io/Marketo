using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Emails.Requests
{
    public class UpdateEmailDynamicContentRequest
    {
        [Display("New HTML content")]
        public string HTMLContent { get; set; }
    }
}
