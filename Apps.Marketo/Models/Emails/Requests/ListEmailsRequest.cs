using Apps.Marketo.DataSourceHandlers;
using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Emails.Requests;

public class ListEmailsRequest
{
    public ListEmailsRequest()
    {

    }
    public ListEmailsRequest(ListEmailsRequest listEmailsRequest, string folderId)
    {
        Status = listEmailsRequest.Status;
        FolderId = folderId;
        EarliestUpdatedAt = listEmailsRequest.EarliestUpdatedAt;
        LatestUpdatedAt = listEmailsRequest.LatestUpdatedAt;
        NamePatterns = listEmailsRequest.NamePatterns;
        ExcludeMatched = listEmailsRequest.ExcludeMatched;
    }

    [DataSource(typeof(StatusDataHandler))]
    public string? Status { get; set; }

    [Display("Folder", Description = "Folders list with \"Email\" type")]
    [DataSource(typeof(EmailFolderDataHandler))]
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