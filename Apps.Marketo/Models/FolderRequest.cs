using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Apps.Marketo.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models;

public class FolderRequest
{
    [Display("Folder ID")]
    [DataSource(typeof(FolderDataHandler))]
    public string FolderId { get; set; }

    [Display("Folder type")]
    [StaticDataSource(typeof(FolderTypeDataHandler))]
    public string FolderType { get; set; }
}