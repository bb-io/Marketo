using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Folder.Requests;

public class ListFoldersRequest
{
    [Display("Root folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string? Root { get; set; }

    [Display("Max depth")]
    public int? MaxDepth { get; set; }

    public string? WorkSpace { get; set; }
}