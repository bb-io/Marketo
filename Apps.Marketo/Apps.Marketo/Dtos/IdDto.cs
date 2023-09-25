using System.Text.Json.Serialization;

namespace Apps.Marketo.Dtos
{
    public class IdDto
    {
        [JsonConverter(typeof(StringConverter))]
        public string Id { get; set; }
    }
}
