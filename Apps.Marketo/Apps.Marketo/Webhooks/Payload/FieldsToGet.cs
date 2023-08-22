using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Webhooks.Payload
{
    public class FieldsToGet
    {
        [Display("Field name 1")]
        public string? Field1 { get; set; }

        [Display("Field name 2")]
        public string? Field2 { get; set; }

        [Display("Field name 3")]
        public string? Field3 { get; set; }

        [Display("Field name 4")]
        public string? Field4 { get; set; }

        [Display("Field name 5")]
        public string? Field5 { get; set; }
    }
}
