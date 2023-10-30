using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Folder.Requests;

public class GetFolderInfoRequest
{
    [Display("Folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string FolderId { get; set; }
}