using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Entities.Email;

public class EmailCCField
{
    [Display("Atrbute ID")]
    public string AttributeId { get; set; }

    [Display("Object name")]
    public string ObjectName { get; set; }

    [Display("Display name")]
    public string DisplayName { get; set; }

    [Display("API name")]
    public string ApiName { get; set; }
}
