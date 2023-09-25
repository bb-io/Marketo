using System.Text.Json.Serialization;

namespace Apps.Marketo.Dtos
{
    public class LandingPageContentDto
    {
        public string Content { get; set; }

        [JsonConverter(typeof(StringConverter))]
        public string Id { get; set; }
    }
}
