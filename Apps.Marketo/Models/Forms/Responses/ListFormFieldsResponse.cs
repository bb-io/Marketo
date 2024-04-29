using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Forms.Responses
{
    public class ListFormFieldsResponse
    {
        [Display("Form fields")]
        public List<string> FormFieldsIds { get; set; }
    }
}
