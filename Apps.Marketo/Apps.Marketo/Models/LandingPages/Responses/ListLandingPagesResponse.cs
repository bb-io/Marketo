using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.LandingPages.Responses
{
    public class ListLandingPagesResponse
    {
        [Display("Landing pages")]
        public IEnumerable<LandingPageDto> LandingPages { get; set; }
    }
}
