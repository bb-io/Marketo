using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Tags.Request
{
    public class GetTagValueRequest
    {
        [Display("Tag value")]
        [DataSource(typeof(TagValueDataHandler))]
        public string TagValue { get; set; }
    }
}
