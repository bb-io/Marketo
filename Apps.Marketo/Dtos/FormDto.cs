using System.Text.Json.Serialization;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Dtos;

public class FormDto
{
    [Display("Form ID")]
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    [Display("Created at")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreatedAt { get; set; }
    
    [JsonConverter(typeof(DateTimeConverter))]
    [Display("Updated at")]
    public DateTime UpdatedAt { get; set; }
    
    public string? Url { get; set; }
    
    public string Status { get; set; }
    
    public string Theme { get; set; }
    
    public string Language { get; set; }
    
    public string? Locale { get; set; }
    
    [Display("Progressive profiling")]
    public bool ProgressiveProfiling { get; set; }
    
    [Display("Label position")]
    public string LabelPosition { get; set; }
    
    [Display("Font family")]
    public string FontFamily { get; set; }
    
    [Display("Font size")]
    public string FontSize { get; set; }
    
    public FormFolderDto Folder { get; set; }
    
    [Display("Known visitor")]
    public KnownVisitorDto KnownVisitor { get; set; }
    
    [Display("Thank you list")]
    public IEnumerable<ThankYouListDto> ThankYouList { get; set; }
    
    [Display("Button location")]
    public int ButtonLocation { get; set; }
    
    [Display("Button label")]
    public string ButtonLabel { get; set; }
    
    [Display("Waiting label")]
    public string WaitingLabel { get; set; }
    
    [Display("Work space ID")]
    public int WorkSpaceId { get; set; }
}

public class FormFolderDto
{
    public string Type { get; set; }
    
    public int Value { get; set; }
    
    [Display("Folder name")]
    public string FolderName { get; set; }
}

public class ThankYouListDto
{
    [Display("Followup type")]
    public string FollowupType { get; set; }
    
    [Display("Followup value")]
    [JsonConverter(typeof(FollowupValueConverter))]
    public object FollowupValue { get; set; }
    
    public bool Default { get; set; }
    
    public string? SubjectField { get; set; }
    
    public string? Operator { get; set; }
    
    public IEnumerable<string>? Values { get; set; }
}

public class KnownVisitorDto
{
    public string Type { get; set; }
    
    public object? Template { get; set; }
}

