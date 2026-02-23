using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.EmailTemplates.Response;

public record ListEmailTemplatesResponse(List<EmailTemplateDto> EmailTemplates)
{
    [Display("Email templates")]
    public List<EmailTemplateDto> EmailTemplates { get; set; } = EmailTemplates;
}
