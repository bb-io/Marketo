using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Identifiers;

public class TagTypeIdentifier
{
    [Display("Tag type"), DataSource(typeof(TagTypeDataHandler))]
    public string TagType { get; set; }
}
