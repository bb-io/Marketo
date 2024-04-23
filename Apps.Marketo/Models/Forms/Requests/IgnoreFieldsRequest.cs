using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Forms.Requests
{
    public class IgnoreFieldsRequest
    {
        [Display("Ignore form fields", Description = "Select fields which you don't want to include in HTML")]
        [DataSource(typeof(FormFieldDataHandler))]
        public List<string>? IgnoreFields { get; set; }

        [Display("Ignore visibility rules content", Description = "Set this field to \"true\" if you don't want to translate visibilty rules content")]
        public bool? IgnoreVisibilityRules { get; set; }
    }
}
