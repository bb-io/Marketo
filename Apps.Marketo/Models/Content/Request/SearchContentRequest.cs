using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Apps.Marketo.DataSourceHandlers.Static;
using Apps.Marketo.Helper.Interfaces;
using Apps.Marketo.Helper.Validator;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Content.Request;

public class SearchContentRequest : IUpdatedDateRange, ICreatedDateRange
{
    [Display("Content types", Description = "Content types to search. Searches all types by default")]
    [StaticDataSource(typeof(ContentTypeDataHandler))]
    public IEnumerable<string>? ContentTypes { get; set; }

    [Display("Status"), StaticDataSource(typeof(StatusDataHandler))]
    public string? Status { get; set; }

    [Display("Folder ID")]
    [DataSource(typeof(SnippetFolderDataHandler))]
    public string? FolderId { get; set; }

    [Display("Created after")]
    public DateTime? CreatedAfter { get; set; }

    [Display("Created before")]
    public DateTime? CreatedBefore { get; set; }

    [Display("Updated after")]
    public DateTime? UpdatedAfter { get; set; }

    [Display("Updated before")]
    public DateTime? UpdatedBefore { get; set; }

    [Display("Name patterns", Description = "Use '*' to represent wildcards in name")]
    public List<string>? NamePatterns { get; set; }

    [Display("Exclude assets matched by patterns", Description = "Exclude assets matched by patterns")]
    public bool? ExcludeMatched { get; set; }

    [Display("Ignore in archive folders")]
    public bool? IgnoreInArchive { get; set; }

    public SearchContentRequest ApplyDefaultValues()
    {
        ContentTypes ??= Constants.ContentTypes.SupportedContentTypes;
        return this;
    }

    public SearchContentRequest Validate()
    {
        this.ValidateDates();
        return this;
    }
}
