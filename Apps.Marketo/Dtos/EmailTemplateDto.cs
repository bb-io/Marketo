using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Marketo.Dtos
{
    public class EmailTemplateDto
    {
        [Display("Created at")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
        public Folder Folder { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }

        [Display("Updated at")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime UpdatedAt { get; set; }
        public string Url { get; set; }
        public int Version { get; set; }
        public string Workspace { get; set; }
    }
}
