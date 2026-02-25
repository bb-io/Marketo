using Newtonsoft.Json;

namespace Apps.Marketo.Models.Entities.Form;

public class KnownVisitor
{
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("template")]
    public string? Template { get; set; } = string.Empty;
}
