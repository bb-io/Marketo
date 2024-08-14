using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.EmailTemplates.Response
{
    public class ListEmailTemplatesResponse
    {
        [Display("Email templates")]
        public List<EmailTemplateDto> EmailTemplates { get; set; }
    }
}
