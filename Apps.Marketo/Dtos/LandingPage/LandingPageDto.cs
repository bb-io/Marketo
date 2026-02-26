using Blackbird.Applications.Sdk.Common;
using Apps.Marketo.Models.Entities.LandingPage;

namespace Apps.Marketo.Dtos.LandingPage;

public class LandingPageDto(LandingPageEntity pageEntity)
{
    [Display("Landing page ID")]
    public string Id { get; set; } = pageEntity.Id;

    [Display("Landing page name")]
    public string Name { get; set; } = pageEntity.Name;

    [Display("Landing page description")]
    public string? Description { get; set; } = !string.IsNullOrEmpty(pageEntity.Description) ? pageEntity.Description : null;

    [Display("Landing page URL")]
    public string Url { get; set; } = pageEntity.Url;

    [Display("Landing page computed URL")]
    public string ComputedUrl { get; set; } = pageEntity.ComputedUrl;

    [Display("Created at")]
    public DateTime CreatedAt { get; set; } = pageEntity.CreatedAt;

    [Display("Updated at")]
    public DateTime UpdatedAt { get; set; } = pageEntity.UpdatedAt;

    [Display("Custom head HTML")]
    public string? CustomHeadHTML { get; set; } = !string.IsNullOrEmpty(pageEntity.CustomHeadHTML) ? pageEntity.CustomHeadHTML : null;

    [Display("Facebook OG tags")]
    public string? FacebookOgTags { get; set; } = !string.IsNullOrEmpty(pageEntity.FacebookOgTags) ? pageEntity.FacebookOgTags : null;

    [Display("Folder ID")]
    public string FolderId { get; set; } = pageEntity.Folder.Value;

    [Display("Form prefill")]
    public bool FormPrefill { get; set; } = pageEntity.FormPrefill;

    [Display("Keywords")]
    public string? Keywords { get; set; } = !string.IsNullOrEmpty(pageEntity.Keywords) ? pageEntity.Keywords : null;

    [Display("Mobile viewing enabled")]
    public bool MobileEnabled { get; set; } = pageEntity.MobileEnabled;

    [Display("Robots")]
    public string Robots { get; set; } = pageEntity.Robots;

    [Display("Landing page status")]
    public string Status { get; set; } = pageEntity.Status;

    [Display("Template ID")]
    public string? TemplateId { get; set; } = !string.IsNullOrEmpty(pageEntity.TemplateId) ? pageEntity.TemplateId : null;

    [Display("Landing page title element")]
    public string? Title { get; set; } = !string.IsNullOrEmpty(pageEntity.Title) ? pageEntity.Title : null;

    [Display("Workspace")]
    public string Workspace { get; set; } = pageEntity.Workspace;
}