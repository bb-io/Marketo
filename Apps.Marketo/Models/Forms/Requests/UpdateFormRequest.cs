using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Forms.Requests
{
    public class UpdateFormRequest
    {
        [Display("Form name", Description = "New form name if it does not exist")]
        public string? FormName { get; set; }
    }
}
