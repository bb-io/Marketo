using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Tokens.Request;

public class CreateTokenRequest
{
    public string Name { get; set; }
    
    [DataSource(typeof(TokenTypeDataHandler))]
    public string Type { get; set; }
    
    public string Value { get; set; }
}