using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Forms.Requests
{
    public class UpdateFormRequest
    {
        [Display("Form name", Description = "New form name if it does not exist")]
        public string? FormName { get; set; }
    }
}
