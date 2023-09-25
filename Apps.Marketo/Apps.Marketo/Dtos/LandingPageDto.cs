using Blackbird.Applications.Sdk.Common;
using System.Text.Json.Serialization;

namespace Apps.Marketo.Dtos
{
    public class LandingPageDto
    {
        public string URL { get; set; }

        [Display("Computed URL")]
        public string ComputedUrl { get; set; }

        [Display("Created at")]
        public string CreatedAt { get; set; }

        [Display("Custom head HTML")]
        public string CustomHeadHTML { get; set; }
        public string Description { get; set; }

        [Display("Facebook OG tags")]
        public string FacebookOgTags { get; set; }
        public Folder Folder { get; set; }

        [Display("Form prefill")]
        public bool FormPrefill { get; set; }

        [JsonConverter(typeof(StringConverter))]
        public string Id { get; set; }
        public string Keywords { get; set; }

        [Display("Mobile enabled")]
        public bool MobileEnabled { get; set; }
        public string Name { get; set; }
        public string Robots { get; set; }
        public string Status { get; set; }
        public int Template { get; set; }
        public string Title { get; set; }

        [Display("Updated at")]
        public string UpdatedAt { get; set; }
        public string Workspace { get; set; }
    }

}
