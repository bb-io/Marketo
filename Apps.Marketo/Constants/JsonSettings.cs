using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Apps.Marketo.Constants;

public static class JsonSettings
{
    public static JsonSerializerSettings SerializerSettings { get; } = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };
}
