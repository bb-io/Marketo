using Newtonsoft.Json;
using Apps.Marketo.Models.Utility.Error;

namespace Apps.Marketo.Dtos;

public class BaseResponseDto<T>
{
    [JsonProperty("errors")]
    public List<Error> Errors { get; set; } = [];

    [JsonProperty("result")]
    public List<T>? Result { get; set; } = [];

    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("warnings")]
    public List<string> Warnings { get; set; } = [];
}