using Apps.Marketo.Dtos.Email;

namespace Apps.Marketo.Models.Emails.Responses;

public record SearchEmailsResponse(List<EmailDto> Emails);