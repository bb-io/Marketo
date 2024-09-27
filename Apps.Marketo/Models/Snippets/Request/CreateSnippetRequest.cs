using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Snippets.Request;

public class CreateSnippetRequest
{
    public string Name { get; set; }
    
    [Display("Folder")]
    [DataSource(typeof(SnippetFolderDataHandler))]
    public string FolderId { get; set; }
    
    public string? Description { get; set; }
}