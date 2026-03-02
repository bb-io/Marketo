using Apps.Marketo.Constants;
using Apps.Marketo.Models.Entities.Email;
using Apps.Marketo.Models.Entities.Form;
using Apps.Marketo.Models.Entities.LandingPage;
using Apps.Marketo.Models.Entities.Snippet;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Marketo.Dtos.Content;

public class ContentDto : IDownloadContentInput
{
    [Display("Content ID")]
    public string ContentId { get; set; } = string.Empty;

    [Display("Content name")]
    public string ContentName { get; set; } = string.Empty;

    [Display("Content type")]
    public string ContentType { get; set; } = string.Empty;

    [Display("Content description")]
    public string? ContentDescription { get; set; }

    [Display("Content URL")]
    public string? ContentUrl { get; set; }

    [Display("Created at")]
    public DateTime CreatedAt { get; set; }

    [Display("Updated at")]
    public DateTime UpdatedAt { get; set; }

    [Display("Status")]
    public string Status { get; set; } = string.Empty;

    [Display("Folder ID")]
    public string FolderId { get; set; } = string.Empty;

    public ContentDto(EmailEntity emailEntity)
    {
        ContentId = emailEntity.Id;
        ContentName = emailEntity.Name;
        ContentType = ContentTypes.Email;
        ContentDescription = string.IsNullOrEmpty(emailEntity.Description) ? null : emailEntity.Description;
        ContentUrl = emailEntity.Url;
        CreatedAt = emailEntity.CreatedAt;
        UpdatedAt = emailEntity.UpdatedAt;
        Status = emailEntity.Status;
        FolderId = emailEntity.Folder.GetCompositeId();
    }

    public ContentDto(LandingPageEntity landingPageEntity)
    {
        ContentId = landingPageEntity.Id;
        ContentName = landingPageEntity.Name;
        ContentType = ContentTypes.LandingPage;
        ContentDescription = string.IsNullOrEmpty(landingPageEntity.Description) ? null : landingPageEntity.Description;
        ContentUrl = landingPageEntity.Url;
        CreatedAt = landingPageEntity.CreatedAt;
        UpdatedAt = landingPageEntity.UpdatedAt;
        Status = landingPageEntity.Status;
        FolderId = landingPageEntity.Folder.GetCompositeId();
    }

    public ContentDto(FormEntity formEntity)
    {
        ContentId = formEntity.Id;
        ContentName = formEntity.Name;
        ContentType = ContentTypes.Form;
        ContentDescription = string.IsNullOrEmpty(formEntity.Description) ? null : formEntity.Description;
        ContentUrl = formEntity.Url;
        CreatedAt = formEntity.CreatedAt;
        UpdatedAt = formEntity.UpdatedAt;
        Status = formEntity.Status;
        FolderId = formEntity.Folder.GetCompositeId();
    }

    public ContentDto(SnippetEntity snippetEntity)
    {
        ContentId = snippetEntity.Id;
        ContentName = snippetEntity.Name;
        ContentType = ContentTypes.Snippet;
        ContentDescription = string.IsNullOrEmpty(snippetEntity.Description) ? null : snippetEntity.Description;
        ContentUrl = snippetEntity.Url;
        CreatedAt = snippetEntity.CreatedAt;
        UpdatedAt = snippetEntity.UpdatedAt;
        Status = snippetEntity.Status;
        FolderId = snippetEntity.Folder.GetCompositeId();
    }
}
