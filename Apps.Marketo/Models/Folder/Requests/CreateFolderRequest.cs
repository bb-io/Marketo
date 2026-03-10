using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Apps.Marketo.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Folder.Requests;

public class CreateFolderRequest
{
    public string Description { get; set; }
    public string Name { get; set; }

    [Display("Parent folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string FolderId { get; set; }

    [Display("Parent folder type")]
    [StaticDataSource(typeof(FolderTypeDataHandler))]
    public string? Type { get; set; }
}