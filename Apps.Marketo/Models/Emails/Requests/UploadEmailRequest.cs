using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Emails.Requests;

public class UploadEmailRequest
{
    [Display("Content")]
    public FileReference File {  get; set; }

    [Display("Segmentation ID"), DataSource(typeof(SegmentationDataHandler))]
    public string? SegmentationId { get; set; }

    [Display("Segment"), DataSource(typeof(SegmentBySegmentationDataHandler))]
    public string? Segment { get; set; }

    [Display("Upload only dynamic content")]
    public bool? UploadOnlyDynamic { get; set; }

    [Display("Recreate corrupted modules", Description = "Marketo sometimes gives random \"System error\" on updating sections with dynamic content.\n" +
    "Blackbird will automatically try to reacreate such module from email template, then copy content from old corrupted module and then translate text content from HTML as usual.\n" +
    "Please be carefull, the ids of content section and the module itself will be changed! You can disable this feature by setting this parameter to false (default value is true)")]
    public bool? RecreateCorruptedModules { get; set; }
    
    [Display("Update email subject", Description = "True by default")]
    public bool? UpdateEmailSubject { get; set; }

    [Display("Update style attributes for images", Description = "False by default")]
    public bool? UpdateStyleForImages { get; set; }
}
