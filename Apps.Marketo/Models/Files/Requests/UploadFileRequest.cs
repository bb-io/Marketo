using Apps.Marketo.DataSourceHandlers.Deprecated;
using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Files.Requests;

public class UploadFileRequest : FileWrapper
{
    public string? Description { get; set; }

    [Display("Insert only")]
    public bool? InsertOnly { get; set; }

    [Display("Folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string FolderId { get; set; }

    [Display("Folder type")]
    [DataSource(typeof(FolderTypeDataHandler))]
    public string Type { get; set; }
}