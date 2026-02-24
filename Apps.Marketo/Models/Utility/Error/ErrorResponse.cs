using Newtonsoft.Json;

namespace Apps.Marketo.Models.Utility.Error;

public class ErrorResponse
{
    [JsonProperty("errors")]
    public List<Error> Errors { get; set; } = [];
}