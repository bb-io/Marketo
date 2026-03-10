using Apps.Marketo.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Marketo.Models.Identifiers.Optional;

public class OptionalPollingContentTypeIdentifier
{
    [Display("Content types"), StaticDataSource(typeof(PollingContentTypeDataHandler))]
    public IEnumerable<string>? ContentTypes { get; set; }

    public OptionalPollingContentTypeIdentifier ApplyDefaultValues()
    {
        ContentTypes ??= Constants.ContentTypes.SupportedPollingContentTypes;
        return this;
    }
}
