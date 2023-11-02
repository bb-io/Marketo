using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Forms.Requests;

public class ListFormsRequest
{
    public int? Hours { get; set; }
    
    [DataSource(typeof(StatusDataHandler))]
    public string? Status { get; set; }
    
    [Display("Folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string? FolderId { get; set; }
}