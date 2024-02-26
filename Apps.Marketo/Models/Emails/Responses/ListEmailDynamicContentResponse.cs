using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Emails.Responses
{
    public class ListEmailDynamicContentResponse
    {
        [Display("Email dynamic content list")]
        public List<GetEmailDynamicContentResponse> EmailDynamicContentList { get; set; }
    }
}
