using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Snippets.Request
{
    public class ListSnippetsRequest
    {
        [DataSource(typeof(StatusDataHandler))]
        public string? Status { get; set; }

        [Display("Start date")]
        public DateTime? StartDate { get; set; }

        [Display("End date")]
        public DateTime? EndDate { get; set; }
    }
}
