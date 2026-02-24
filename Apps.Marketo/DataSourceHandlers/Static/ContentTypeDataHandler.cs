using Apps.Marketo.Constants;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.DataSourceHandlers.Static;

public class ContentTypeDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return ContentTypes.SupportedContentTypes.Select(x => new DataSourceItem(x, x));
    }
}
