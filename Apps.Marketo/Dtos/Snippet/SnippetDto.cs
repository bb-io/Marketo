using Blackbird.Applications.Sdk.Common;
using Apps.Marketo.Models.Entities.Snippet;

namespace Apps.Marketo.Dtos.Snippet;

public class SnippetDto(SnippetEntity snippetEntity)
{
    [Display("Snippet ID")]
    public string Id { get; set; } = snippetEntity.Id;

    [Display("Snippet name")]
    public string Name { get; set; } = snippetEntity.Name;

    [Display("Snippet description")]
    public string? Description { get; set; } = snippetEntity.Description;

    [Display("Snippet URL")]
    public string Url { get; set; } = snippetEntity.Url;

    [Display("Folder ID")]
    public string FolderId { get; set; } = snippetEntity.Folder.GetCompositeId();

    [Display("Snippet status")]
    public string Status { get; set; } = snippetEntity.Status;

    [Display("Workspace")]
    public string Workspace { get; set; } = snippetEntity.Workspace;

    [Display("Created at")]
    public DateTime CreatedAt { get; set; } = snippetEntity.CreatedAt;

    [Display("Updated at")]
    public DateTime UpdatedAt { get; set; } = snippetEntity.UpdatedAt;
}