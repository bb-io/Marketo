using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Dtos;

public class TokenDto
{
    public string Name { get; set; }
    
    public string Type { get; set; }
    public string Value { get; set; }
    
    [Display("Computed URL")]
    public string ComputedUrl { get; set; }
}