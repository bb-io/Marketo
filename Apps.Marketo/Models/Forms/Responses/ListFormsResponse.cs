using Apps.Marketo.Dtos;

namespace Apps.Marketo.Models.Forms.Responses;

public class ListFormsResponse
{
    public IEnumerable<FormDto> Forms { get; set; }
}