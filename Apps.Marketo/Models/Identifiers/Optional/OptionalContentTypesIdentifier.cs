using Apps.Marketo.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Marketo.Models.Identifiers.Optional;

public class OptionalContentTypesIdentifier
{
    [Display("Content types", Description = "Content types to search. Searches all types by default")]
    [StaticDataSource(typeof(ContentTypeDataHandler))]
    public IEnumerable<string>? ContentTypes { get; set; }

    public OptionalContentTypesIdentifier ApplyDefaultValues()
    {
        ContentTypes ??= Constants.ContentTypes.SupportedContentTypes;
        return this;
    }

}
