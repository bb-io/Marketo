using Newtonsoft.Json;

namespace Apps.Marketo.Models.Entities.Segmentation;

public class SegmenationEntity
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
}
