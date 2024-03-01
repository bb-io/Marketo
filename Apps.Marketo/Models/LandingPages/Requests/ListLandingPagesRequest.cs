using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.LandingPages.Requests;

public class ListLandingPagesRequest
{
    [DataSource(typeof(StatusDataHandler))]
    public string? Status { get; set; }

    [Display("Folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string? FolderId { get; set; }

    [Display("Folder type")]
    [DataSource(typeof(FolderTypeDataHandler))]
    public string? Type { get; set; }

    [Display("Start date")]
    public DateTime? StartDate { get; set; }

    [Display("End date")]
    public DateTime? EndDate { get; set; }
}