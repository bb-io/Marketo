using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Tags.Request;

public class GetProgramTagRequest
{
    [Display("Ignore tag not found error", Description = "By default, this is set to false. If set to true, the action will not throw an error but will return an empty string.")]
    public bool? IgnoreTagNotFoundError { get; set; }
}