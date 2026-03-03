using Apps.Marketo.Models.Content.Request;

namespace Apps.Marketo.Services.Content.Models;

public record UploadContentInput
{
    public UploadContentInput(string htmlContent, UploadContentRequest uploadRequest)
    {
        HtmlContent = htmlContent;
        ContentId = uploadRequest.ContentId;
        SegmentationId = uploadRequest.SegmentationId;
        Segment = uploadRequest.Locale;
        UploadOnlyDynamicContent = uploadRequest.UploadOnlyDynamic ?? false;
        RecreateCorruptedEmailModules = uploadRequest.RecreateCorruptedModules ?? true;
        UpdateEmailSubject = uploadRequest.UpdateEmailSubject ?? true;
        UpdateStyleForEmailImages = uploadRequest.UpdateStyleForImages ?? false;
    }

    public string HtmlContent { get; set; }

    public string? ContentId { get; set; }

    public string? SegmentationId { get; set; }

    public string? Segment { get; set; }

    public bool? UploadOnlyDynamicContent { get; set; }

    public bool? RecreateCorruptedEmailModules { get; set; }

    public bool? UpdateEmailSubject { get; set; }

    public bool? UpdateStyleForEmailImages { get; set; }
}
