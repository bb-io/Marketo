using Newtonsoft.Json;

namespace Apps.Marketo.Models.Entities.LandingPage;

public class LandingPageEntity
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("URL")]
    public string Url { get; set; } = string.Empty;

    [JsonProperty("computedUrl")]
    public string ComputedUrl { get; set; } = string.Empty;

    [JsonProperty("createdAt"), JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt"), JsonConverter(typeof(DateTimeConverter))]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("customHeadHTML")]
    public string? CustomHeadHTML { get; set; }

    [JsonProperty("facebookOgTags")]
    public string? FacebookOgTags { get; set; }

    [JsonProperty("folder")]
    public AssetFolder Folder { get; set; } = null!;

    [JsonProperty("formPrefill")]
    public bool FormPrefill { get; set; }

    [JsonProperty("keywords")]
    public string? Keywords { get; set; }

    [JsonProperty("mobileEnabled")]
    public bool MobileEnabled { get; set; }

    [JsonProperty("robots")]
    public string Robots { get; set; } = string.Empty;

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("templateId")]
    public string? TemplateId { get; set; }

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("workspace")]
    public string Workspace { get; set; } = string.Empty;
}
