using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Apps.Marketo.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Content.Request;

public class SearchContentRequest
{
    [Display("Status"), StaticDataSource(typeof(StatusDataHandler))]
    public string? Status { get; set; }

    [Display("Folder ID")]
    [DataSource(typeof(SnippetFolderDataHandler))]
    public string? FolderId { get; set; }

    [Display("Earliest updated at")]
    public DateTime? EarliestUpdatedAt { get; set; }

    [Display("Latest updated at")]
    public DateTime? LatestUpdatedAt { get; set; }

    [Display("Name patterns", Description = "Use '*' to represent wildcards in name")]
    public List<string>? NamePatterns { get; set; }

    [Display("Exclude assets matched by patterns", Description = "Exclude assets matched by patterns")]
    public bool? ExcludeMatched { get; set; }

    [Display("Content types", Description = "Content types to search. Searches all types by default")]
    [StaticDataSource(typeof(ContentTypeDataHandler))]
    public IEnumerable<string>? ContentTypes { get; set; }

    public void ApplyDefaultValues()
    {
        ContentTypes ??= Constants.ContentTypes.SupportedContentTypes;
    }
}
