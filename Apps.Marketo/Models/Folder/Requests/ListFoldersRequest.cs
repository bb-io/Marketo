using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Folder.Requests;

public class ListFoldersRequest
{
    [Display("Root folder ID")]
    [DataSource(typeof(FolderDataHandler))]
    public string? Root { get; set; }

    [Display("Manual root folder ID")]
    public string? RootManual { get; set; }

    [Display("Max depth")]
    public int? MaxDepth { get; set; }

    public string? WorkSpace { get; set; }

    [Display("Filter folder types", Description = "If set, the results will be filtered to match one of the folder types")]
    public List<string>? FilterFolderTypes { get; set; }
}