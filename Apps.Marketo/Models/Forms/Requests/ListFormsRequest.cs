using Apps.Marketo.DataSourceHandlers;
using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Forms.Requests;

public class ListFormsRequest
{
    [Display("Earliest updated at")]
    public DateTime? EarliestUpdatedAt { get; set; }
    
    [Display("Latest updated at")]
    public DateTime? LatestUpdatedAt { get; set; }
    
    [DataSource(typeof(StatusDataHandler))]
    public string? Status { get; set; }
    
    [Display("Folder", Description = "Folders list with \"Landing Page Form\" type")]
    [DataSource(typeof(FolderDataHandler))]
    public string? FolderId { get; set; }
}