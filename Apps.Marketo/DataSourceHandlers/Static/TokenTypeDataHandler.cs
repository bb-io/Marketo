using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Marketo.DataSourceHandlers.Static;

public class TokenTypeDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return
        [
            new DataSourceItem("date", "Date"),
            new DataSourceItem("number", "Number"),
            new DataSourceItem("text", "Text"),
            new DataSourceItem("rich text", "Rich text"),
            new DataSourceItem("score", "Score"),
            new DataSourceItem("sfdc campaign", "Salesforce campaign"),
        ];
    }
}