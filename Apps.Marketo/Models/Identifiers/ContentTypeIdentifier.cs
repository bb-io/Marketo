using Apps.Marketo.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Marketo.Models.Identifiers;

public class ContentTypeIdentifier
{
    [Display("Content type"), StaticDataSource(typeof(ContentTypeDataHandler))]
    public string ContentType { get; set; }
}
