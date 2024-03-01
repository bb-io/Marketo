using Blackbird.Applications.Sdk.Common;
using System.Text.Json.Serialization;

namespace Apps.Marketo.Dtos;

public class SnippetDto
{
    [Display("Snippet ID")]
    public string Id { get; set; }

    [Display("Name")]
    public string Name { get; set; }

    [Display("Description")]
    public string Description { get; set; }

    [Display("URL")]
    public string Url { get; set; }
    
    public FormFolderDto Folder { get; set; }

    [Display("Status")]
    public string Status { get; set; }

    [Display("Workspace")]
    public string Workspace { get; set; }

    [Display("Created at")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreatedAt { get; set; }

    [Display("Updated at")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime UpdatedAt { get; set; }
}