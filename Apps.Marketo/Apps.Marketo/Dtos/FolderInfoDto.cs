using Blackbird.Applications.Sdk.Common;
using System.Text.Json.Serialization;

namespace Apps.Marketo.Dtos
{
    public class FolderInfoDto
    {
        [JsonConverter(typeof(StringConverter))]
        [Display("Access zone ID")]
        public string AccessZoneId { get; set; }

        [Display("Created at")]
        public string CreatedAt { get; set; }
        public string Description { get; set; }

        [Display("Folder")]
        public Folder FolderId { get; set; }

        [Display("Folder type")]
        public string FolderType { get; set; }

        [JsonConverter(typeof(StringConverter))]
        public string Id { get; set; }

        [Display("Is archive")]
        public bool IsArchive { get; set; }

        [Display("Is system")]
        public bool IsSystem { get; set; }
        public string Name { get; set; }
        public Folder Folder { get; set; }
        public string Path { get; set; }

        [Display("Updated at")]
        public string UpdatedAt { get; set; }
        public string Url { get; set; }
        public string Workspace { get; set; }
    }

    public class Folder {
        [JsonConverter(typeof(StringConverter))]
        public string Id { get; set; }
        public string Type { get; set; }
    }
}
