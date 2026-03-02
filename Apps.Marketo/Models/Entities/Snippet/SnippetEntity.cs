using Newtonsoft.Json;

namespace Apps.Marketo.Models.Entities.Snippet;

public class SnippetEntity
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;

    [JsonProperty("folder")]
    public AssetFolder Folder { get; set; } = null!;

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("workspace")]
    public string Workspace { get; set; } = string.Empty;

    [JsonProperty("createdAt"), JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt"), JsonConverter(typeof(DateTimeConverter))]
    public DateTime UpdatedAt { get; set; }
}
