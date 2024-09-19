using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.EmailTemplates.Requests
{
    public class UpdateEmailTemplateContentRequest
    {
        [Display("Content")]
        public string Content { get; set; }
    }
}
