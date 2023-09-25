using Blackbird.Applications.Sdk.Utils.Sdk.DataSourceHandlers;

namespace Apps.Marketo.DataSourceHandlers
{
    public class FolderTypeDataHandler : EnumDataHandler 
    {
        protected override Dictionary<string, string> EnumValues => new()
        {
            { "Folder", "Folder" },
            { "Program", "Program" },
        };
    }
}
