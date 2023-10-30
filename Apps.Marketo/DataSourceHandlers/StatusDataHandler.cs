using Blackbird.Applications.Sdk.Utils.Sdk.DataSourceHandlers;

namespace Apps.Marketo.DataSourceHandlers;

public class StatusDataHandler : EnumDataHandler
{
    protected override Dictionary<string, string> EnumValues => new()
    {
        { "approved", "Approved" },
        { "draft", "Draft" },
    };
}