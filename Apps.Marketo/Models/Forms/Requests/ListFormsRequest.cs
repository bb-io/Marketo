using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Forms.Requests;

public class ListFormsRequest
{
    [Display("Start date")]
    public DateTime StartDate { get; set; }
    
    [Display("End date")]
    public DateTime? EndDate { get; set; }
    
    [DataSource(typeof(StatusDataHandler))]
    public string? Status { get; set; }
    
    [Display("Folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string? FolderId { get; set; }
}