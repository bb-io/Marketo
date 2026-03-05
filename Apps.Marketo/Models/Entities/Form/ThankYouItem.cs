using Newtonsoft.Json;

namespace Apps.Marketo.Models.Entities.Form;

public class ThankYouItem
{
    [JsonProperty("followupType")]
    public string FollowupType { get; set; } = string.Empty;

    [JsonProperty("followupValue")]
    public string FollowupValue { get; set; } = string.Empty;

    [JsonProperty("default")]
    public bool Default { get; set; }

    [JsonProperty("subjectField")]
    public string? SubjectField { get; set; }

    [JsonProperty("operator")]
    public string? Operator { get; set; }

    [JsonProperty("values")]
    public IEnumerable<string>? Values { get; set; }
}
