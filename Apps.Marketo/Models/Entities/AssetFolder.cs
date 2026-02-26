using Newtonsoft.Json;

namespace Apps.Marketo.Models.Entities;

public class AssetFolder
{
    [JsonProperty("value")]
    public string Value { get; set; } = string.Empty;

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;
}