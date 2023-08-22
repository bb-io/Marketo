using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.LandingPages.Responses
{
    public class ListLandingPagesResponse
    {
        [Display("Landing pages")]
        public IEnumerable<LandingPageDto> LandingPages { get; set; }
    }
}
