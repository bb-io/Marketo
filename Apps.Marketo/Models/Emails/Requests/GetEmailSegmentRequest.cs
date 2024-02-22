using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Emails.Requests
{
    public class GetEmailSegmentRequest
    {
        [Display("Segment")]
        [DataSource(typeof(EmailSegmentDataHandler))]
        public string Segment { get; set; }
    }
}
