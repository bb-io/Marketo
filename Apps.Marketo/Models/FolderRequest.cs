using Apps.Marketo.DataSourceHandlers.Deprecated;
using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models;

public class FolderRequest
{
    [Display("Folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string FolderId { get; set; }

    [Display("Folder type")]
    [DataSource(typeof(FolderTypeDataHandler))]
    public string FolderType { get; set; }
}