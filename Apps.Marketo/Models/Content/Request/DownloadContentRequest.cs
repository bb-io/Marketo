using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Marketo.Models.Content.Request;

public class DownloadContentRequest : IDownloadContentInput
{
    [Display("Content ID"), DataSource(typeof(ContentDataHandler))]
    public string ContentId { get; set; }

    [Display("Segmentation ID (required for emails, landing pages and snippets)"), DataSource(typeof(SegmentationDataHandler))]
    public string SegmentationId { get; set; }

    [Display("Segment (required for emails, landing pages and snippets)"), DataSource(typeof(SegmentBySegmentationDataHandler))]
    public string Segment { get; set; }

    [Display("Get only dynamic content (for emails and landing pages)")]
    public bool? GetOnlyDynamicContent { get; set; }

    [Display("Include images (for emails and landing pages)")]
    public bool? IncludeImages { get; set; }

    [Display("Ignore form fields"), DataSource(typeof(FormFieldDataHandler))]
    public List<string>? IgnoreFormFields { get; set; }

    [Display("Ignore form visibility rules content")]
    public bool? IgnoreVisibilityRules { get; set; }
}
