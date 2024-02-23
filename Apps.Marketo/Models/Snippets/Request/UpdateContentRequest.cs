using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Snippets.Request;

public class UpdateContentRequest
{
    [DataSource(typeof(ContentTypeHandler))]
    public string Type { get; set; }
    
    public string Content { get; set; }
}