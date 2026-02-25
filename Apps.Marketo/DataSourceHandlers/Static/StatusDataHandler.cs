using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Marketo.DataSourceHandlers.Static;

public class StatusDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return 
        [
            new DataSourceItem("approved", "Approved"),
            new DataSourceItem("draft", "Draft"),
        ];
    }
}