using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Snippets.Request;

public class SnippetOptionalRequest
{
    [Display("Snippet ID")]
    [DataSource(typeof(SnippetDataHandler))]
    public string? SnippetId { get; set; }
}