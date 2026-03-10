using Newtonsoft.Json;

namespace Apps.Marketo.Models.Utility.Error;

public class AuthError
{
    [JsonProperty("error")]
    public string Error { get; set; } = string.Empty;

    [JsonProperty("error_description")]
    public string ErrorDescription { get; set; } = string.Empty;
}
