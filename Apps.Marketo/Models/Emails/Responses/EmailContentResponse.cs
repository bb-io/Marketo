using Apps.Marketo.Dtos;

namespace Apps.Marketo.Models.Emails.Responses;

public class EmailContentResponse
{
    public EmailContentResponse(List<EmailContentDto> contentItems)
    {
        EmailContentItems = contentItems;
    }

    public List<EmailContentDto> EmailContentItems { get; set; }
}