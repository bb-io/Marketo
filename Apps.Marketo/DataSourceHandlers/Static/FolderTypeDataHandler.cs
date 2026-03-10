using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Marketo.DataSourceHandlers.Static;

public class FolderTypeDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return
        [
            new DataSourceItem("Folder", "Folder"),
            new DataSourceItem("Program", "Program"),
        ];
    }
}