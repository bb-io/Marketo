using Blackbird.Applications.Sdk.Common;
using Apps.Marketo.Models.Entities.Email;

namespace Apps.Marketo.Dtos.Email;

public class EmailDto(EmailEntity emailEntity)
{
    [Display("Email ID")]
    public string Id { get; set; } = emailEntity.Id;

    [Display("Email name")]
    public string Name { get; set; } = emailEntity.Name;

    [Display("Email description")]
    public string? Description { get; set; } = !string.IsNullOrEmpty(emailEntity.Description) ? emailEntity.Description : null;

    [Display("Created at")]
    public DateTime CreatedAt { get; set; } = emailEntity.CreatedAt;

    [Display("Updated at")]
    public DateTime UpdatedAt { get; set; } = emailEntity.UpdatedAt;

    [Display("Folder ID")]
    public string FolderId { get; set; } = emailEntity.Folder.GetCompositeId();

    [Display("Sender email")]
    public string SenderEmail { get; set; } = emailEntity.FromEmail.Value;

    [Display("Sender name")]
    public string SenderName { get; set; } = emailEntity.FromName.Value;

    [Display("Operational")]
    public bool Operational { get; set; } = emailEntity.Operational;

    [Display("Published to Marketo Sales Insight")]
    public bool PublishToMSI { get; set; } = emailEntity.PublishToMSI;

    [Display("Reply email")]
    public string ReplyEmail { get; set; } = emailEntity.ReplyEmail.Value;

    [Display("Email status")]
    public string Status { get; set; } = emailEntity.Status;

    [Display("Email subject")]
    public string? Subject { get; set; } = !string.IsNullOrEmpty(emailEntity.Subject.Value) ? emailEntity.Subject.Value : null;

    [Display("Email preheader")]
    public string? PreHeader { get; set; } = emailEntity.PreHeader;

    [Display("Template ID")]
    public string TemplateId { get; set; } = emailEntity.TemplateId;

    [Display("Text only")]
    public bool TextOnly { get; set; } = emailEntity.TextOnly;

    [Display("Email URL")]
    public string Url { get; set; } = emailEntity.Url;

    [Display("Email version")]
    public int Version { get; set; } = emailEntity.Version;

    [Display("'View as Webpage' function is enabled")]
    public bool WebView { get; set; } = emailEntity.WebView;

    [Display("Workspace name")]
    public string WorkspaceName { get; set; } = emailEntity.WorkspaceName;

    [Display("Auto copy to text enabled")]
    public bool AutoCopyToText { get; set; } = emailEntity.AutoCopyToText;

    [Display("CC Fields")]
    public List<string> CCFields { get; set; } = emailEntity.CCFields.Select(x => x.DisplayName).ToList();
}
