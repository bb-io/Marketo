using Apps.Marketo.Dtos;

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
