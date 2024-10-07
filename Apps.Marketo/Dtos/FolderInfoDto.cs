using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Marketo.Dtos;

public class FolderInfoDto
{
    
    [Display("Access zone ID")]
    public string AccessZoneId { get; set; }

    [Display("Created at")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreatedAt { get; set; }
    public string Description { get; set; }

    [Display("Folder")]
    public Folder FolderId { get; set; }

    [Display("Parent folder")]
    public Folder? Parent { get; set; }

    [Display("Folder type")]
    public string FolderType { get; set; }

    
    public string Id { get; set; }

    [Display("Is archive")]
    public bool IsArchive { get; set; }

    [Display("Is system")]
    public bool IsSystem { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }

    [Display("Updated at")]
    public string UpdatedAt { get; set; }
    public string Url { get; set; }
    public string Workspace { get; set; }

    [Display("Search ID", Description = "Pass this ID in \"Search\" action")]
    public string SearchId { get; set; }
}

public class Folder {

    [Display("ID")]
    public string Id { get; set; }
    public string Type { get; set; }
}