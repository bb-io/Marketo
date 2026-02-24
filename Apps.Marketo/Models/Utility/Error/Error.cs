using Newtonsoft.Json;

namespace Apps.Marketo.Models.Utility.Error;

public class Error
{
    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("code")]
    public string Code { get; set; } = string.Empty;
}