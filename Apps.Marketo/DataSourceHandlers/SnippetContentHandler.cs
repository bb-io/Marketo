using Blackbird.Applications.Sdk.Utils.Sdk.DataSourceHandlers;

namespace Apps.Marketo.DataSourceHandlers;

public class SnippetContentHandler : EnumDataHandler
{
    protected override Dictionary<string, string> EnumValues => new()
    {
        ["HTML"] = "HTML",
        ["Text"] = "Text",
        ["DynamicContent"] = "Dynamic content",
    };
}