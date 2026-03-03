using Apps.Marketo.DataSourceHandlers;
using Apps.Marketo.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Marketo.Models.Content.Request;

public class UploadContentRequest : IUploadContentInput
{
    [Display("Content")]
    public FileReference Content { get; set ; }

    [Display("Segmentation ID (for emails, landing pages and snippets)"), DataSource(typeof(SegmentationDataHandler))]
    public string? SegmentationId { get; set; }

    [Display("Segment (for emails, landing pages and snippets)"), DataSource(typeof(SegmentBySegmentationDataHandler))]
    public string? Locale { get; set; }

    [Display("Content type"), StaticDataSource(typeof(ContentTypeDataHandler))]
    public string? ContentType { get; set; }

    [Display("Content ID"), DataSource(typeof(ContentDataHandler))]
    public string? ContentId { get ; set ; }

    [Display("Upload only dynamic email content")]
    public bool? UploadOnlyDynamic { get; set; }

    [Display("Recreate corrupted email modules", 
        Description = "Marketo sometimes gives random \"System error\" on updating sections with dynamic content.\n" +
        "Blackbird will automatically try to reacreate such module from email template, " +
        "then copy content from old corrupted module and then translate text content from HTML as usual.\n" +
        "Please be carefull, the ids of content section and the module itself will be changed! " +
        "You can disable this feature by setting this parameter to false (default value is true)")]
    public bool? RecreateCorruptedModules { get; set; }

    [Display("Update email subject", Description = "True by default")]
    public bool? UpdateEmailSubject { get; set; }

    [Display("Update style attributes for email images", Description = "False by default")]
    public bool? UpdateStyleForImages { get; set; }
}
