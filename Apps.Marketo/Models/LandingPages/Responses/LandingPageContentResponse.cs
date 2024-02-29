using Apps.Marketo.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.LandingPages.Responses
{
    public class LandingPageContentResponse
    {
        public LandingPageContentResponse(List<LandingPageContentDto> contentItems)
        {
            LandingPageContentItems = contentItems;
        }

        public List<LandingPageContentDto> LandingPageContentItems { get; set; }
    }
}
