using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Snippets.Request;

public class CreateSnippetRequest
{
    public string Name { get; set; }
    
    [Display("Folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string FolderId { get; set; }

    [Display("Folder type")]
    [DataSource(typeof(FolderTypeDataHandler))]
    public string FolderType { get; set; }
    
    public string? Description { get; set; }
}