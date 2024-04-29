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
    public class GetMultipleFormsRequest
    {
        [Display("Forms")]
        [DataSource(typeof(FormDataHandler))]
        public List<string> Forms { get; set; }
    }
}
