using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.LandingPages.Requests;

public class GetLandingInfoOptionalRequest
{
    [DataSource(typeof(LandingPageHandler))]
    [Display("Landing page ID")]
    public string? Id { get; set; }
}