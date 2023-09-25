using Blackbird.Applications.Sdk.Common;
using System.Text.Json.Serialization;

namespace Apps.Marketo.Dtos
{
    public class LandingPageTemplateDto
    {
        [Display("Created at")]
        public string CreatedAt { get; set; }
        public string Description { get; set; }

        [Display("Enable munchkin")]
        public bool EnableMunchkin { get; set; }
        public Folder Folder { get; set; }

        [JsonConverter(typeof(StringConverter))]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }

        [Display("Template type")]
        public string TemplateType { get; set; }

        [Display("Updated at")]
        public string UpdatedAt { get; set; }
        public string Url { get; set; }
        public string Workspace { get; set; }
    }
}
