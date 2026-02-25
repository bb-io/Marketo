using Apps.Marketo.Dtos;

namespace Apps.Marketo.Models.Forms.Responses;

public record SearchFormsResponse(List<FormDto> Forms);