using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.LandingPages.Responses;

public record SearchLandingPagesResponse(List<LandingPageDto> LandingPages)
{
    [Display("Landing pages")]
    public List<LandingPageDto> LandingPages { get; set; } = LandingPages;
}