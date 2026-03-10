using Newtonsoft.Json;

namespace Apps.Marketo.Dtos;

public class LandingPageContentDto
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("index")]
    public int Index { get; set; }

    [JsonProperty("content")]
    public object Content { get; set; }

    [JsonProperty("formattingOptions")]
    public FormattingOptions FormattingOptions { get; set; }
}