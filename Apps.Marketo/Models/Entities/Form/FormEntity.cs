using Newtonsoft.Json;
using Apps.Marketo.Helper.Interfaces;

namespace Apps.Marketo.Models.Entities.Form;

public class FormEntity : IEntityName, IEntityUpdatedAt, IEntityCreatedAt, IEntityFolder
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("createdAt"), JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt"), JsonConverter(typeof(DateTimeConverter))]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("url")]
    public string? Url { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("theme")]
    public string Theme { get; set; } = string.Empty;

    [JsonProperty("language")]
    public string Language { get; set; } = string.Empty;

    [JsonProperty("locale")]
    public string? Locale { get; set; }

    [JsonProperty("progressiveProfiling")]
    public bool ProgressiveProfiling { get; set; }

    [JsonProperty("labelPosition")]
    public string LabelPosition { get; set; } = string.Empty;

    [JsonProperty("fontFamily")]
    public string FontFamily { get; set; } = string.Empty;

    [JsonProperty("fontSize")]
    public string FontSize { get; set; } = string.Empty;

    [JsonProperty("folder")]
    public AssetFolder Folder { get; set; } = null!;

    [JsonProperty("knownVisitor")]
    public KnownVisitor KnownVisitor { get; set; }

    [JsonProperty("buttonLocation")]
    public int ButtonLocation { get; set; }

    [JsonProperty("buttonLabel")]
    public string ButtonLabel { get; set; } = string.Empty;

    [JsonProperty("waitingLabel")]
    public string WaitingLabel { get; set; } = string.Empty;

    [JsonProperty("workspaceId")]
    public string WorkSpaceId { get; set; } = string.Empty;

    [JsonProperty("thankYouList")]
    public List<ThankYouItem> ThankYouList { get; set; } = [];
}
