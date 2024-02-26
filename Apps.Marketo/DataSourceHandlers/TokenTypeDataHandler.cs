using Blackbird.Applications.Sdk.Utils.Sdk.DataSourceHandlers;

namespace Apps.Marketo.DataSourceHandlers;

public class TokenTypeDataHandler : EnumDataHandler
{
    protected override Dictionary<string, string> EnumValues => new()
    {
        {"date", "Date"},
        {"number", "Number"},
        {"text", "Text"},
        {"rich text", "Rich text"},
        {"score", "Score"},
        {"sfdc campaign", "Salesforce campaign"},
    };
}