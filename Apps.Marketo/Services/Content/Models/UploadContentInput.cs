using Apps.Marketo.Models.Content.Request;
using Apps.Marketo.Models.Emails.Requests;
using Apps.Marketo.Models.Identifiers.Optional;
using Apps.Marketo.Models.Snippets.Request;

namespace Apps.Marketo.Services.Content.Models;

public record UploadContentInput
{
    public UploadContentInput(string htmlContent, OptionalSnippetIdentifier snippetInput, UploadSnippetRequest uploadRequest)
    {
        HtmlContent = htmlContent;
        ContentId = snippetInput.SnippetId;
        SegmentationId = uploadRequest.SegmentationId;
        Segment = uploadRequest.Segment;
    }

    public UploadContentInput(string htmlContent, OptionalEmailIdenfitier emailInput, UploadEmailRequest uploadRequest)
    {
        HtmlContent = htmlContent;
        ContentId = emailInput.EmailId;
        SegmentationId = uploadRequest.SegmentationId;
        Segment = uploadRequest.Segment;
        UploadOnlyDynamicContent = uploadRequest.UploadOnlyDynamic ?? false;
        RecreateCorruptedEmailModules = uploadRequest.RecreateCorruptedModules ?? true;
        UpdateEmailSubject = uploadRequest.UpdateEmailSubject ?? true;
        UpdateStyleForEmailImages = uploadRequest.UpdateStyleForImages ?? false;
    }

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
