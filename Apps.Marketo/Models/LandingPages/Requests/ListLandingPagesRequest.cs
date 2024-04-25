using Apps.Marketo.DataSourceHandlers;
using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.LandingPages.Requests;

public class ListLandingPagesRequest
{
    [DataSource(typeof(StatusDataHandler))]
    public string? Status { get; set; }

    [Display("Folder", Description = "Folders list with \"Landing Page\" type")]
    [DataSource(typeof(LandingPageFolderDataHandler))]
    public string? FolderId { get; set; }

    [Display("Earliest updated at")]
    public DateTime? EarliestUpdatedAt { get; set; }

    [Display("Latest updated at")]
    public DateTime? LatestUpdatedAt { get; set; }

    [Display("Name patterns", Description = "Use '*' to represent wildcards in name")]
    public List<string>? NamePatterns { get; set; }

    [Display("Exclude assets matched by patterns", Description = "Exclude assets matched by patterns")]
    public bool? ExcludeMatched { get; set; }
}