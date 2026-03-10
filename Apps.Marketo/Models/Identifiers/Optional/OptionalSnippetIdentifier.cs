using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Identifiers.Optional;

public class OptionalSnippetIdentifier
{
    [Display("Snippet ID"), DataSource(typeof(SnippetDataHandler))]
    public string? SnippetId { get; set; }
}