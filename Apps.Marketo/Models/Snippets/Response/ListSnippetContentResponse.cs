using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Snippets.Response;

public class ListSnippetContentResponse
{
    [Display("Content items")]
    public IEnumerable<SnippetContentDto> ContentItems { get; set; }
}