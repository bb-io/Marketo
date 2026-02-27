using Blackbird.Applications.Sdk.Common;
using Apps.Marketo.Models.Entities.Form;

namespace Apps.Marketo.Dtos.Form;

public class FormDto(FormEntity formEntity)
{
    [Display("Form ID")]
    public string Id { get; set; } = formEntity.Id;

    [Display("Form name")]
    public string Name { get; set; } = formEntity.Name;

    [Display("Form description")]
    public string? Description { get; set; } = formEntity.Description;

    [Display("Created at")]
    public DateTime CreatedAt { get; set; } = formEntity.CreatedAt;
    
    [Display("Updated at")]
    public DateTime UpdatedAt { get; set; } = formEntity.UpdatedAt;

    [Display("Form URL")]
    public string? Url { get; set; } = formEntity.Url;

    [Display("Form status")]
    public string Status { get; set; } = formEntity.Status;

    [Display("Form theme")]
    public string Theme { get; set; } = formEntity.Theme;

    [Display("Form language")]
    public string Language { get; set; } = formEntity.Language;

    [Display("Form locale")]
    public string? Locale { get; set; } = formEntity.Locale;

    [Display("Progressive profiling")]
    public bool ProgressiveProfiling { get; set; } = formEntity.ProgressiveProfiling;

    [Display("Label position")]
    public string LabelPosition { get; set; } = formEntity.LabelPosition;

    [Display("Font family")]
    public string FontFamily { get; set; } = formEntity.FontFamily;

    [Display("Font size")]
    public string FontSize { get; set; } = formEntity.FontSize;

    [Display("Folder ID")]
    public string FolderId { get; set; } = formEntity.Folder.GetCompositeId();

    [Display("Known visitor template")]
    public string? KnownVisitorTemplate { get; set; } = formEntity.KnownVisitor.Template;

    [Display("Button location")]
    public int ButtonLocation { get; set; } = formEntity.ButtonLocation;

    [Display("Button label")]
    public string ButtonLabel { get; set; } = formEntity.ButtonLabel;

    [Display("Waiting label")]
    public string WaitingLabel { get; set; } = formEntity.WaitingLabel;

    [Display("Work space ID")]
    public string WorkSpaceId { get; set; } = formEntity.WorkSpaceId;
}