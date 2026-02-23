using Apps.Marketo.Dtos;

namespace Apps.Marketo.Models.Emails.Responses;

public record ListEmailsResponse(List<EmailDto> Emails);