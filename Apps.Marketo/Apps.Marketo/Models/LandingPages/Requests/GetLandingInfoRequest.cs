using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.LandingPages.Requests
{
    public class GetLandingInfoRequest
    {
        [DataSource(typeof(LandingPageHandler))]
        [Display("Landing page")]
        public string Id { get; set; }
    }
}
