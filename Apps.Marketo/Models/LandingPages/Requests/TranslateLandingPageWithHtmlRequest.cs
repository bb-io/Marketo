using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Marketo.Models.LandingPages.Requests;
public class TranslateLandingPageWithHtmlRequest
{
    [Display("Translated HTML file")]
    public FileReference File { get; set; }

    [Display("Translate only dynamic content")]
    public bool? TranslateOnlyDynamic { get; set; }
}

