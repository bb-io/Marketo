using Apps.Marketo.Dtos.Form;

namespace Apps.Marketo.Models.Forms.Responses;

public record SearchFormsResponse(List<FormDto> Forms);