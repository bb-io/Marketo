using Apps.Marketo.Constants;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.DataSourceHandlers.Static;

public class PollingContentTypeDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return ContentTypes.SupportedPollingContentTypes.Select(x => new DataSourceItem(x, x)).ToList();
    }
}
