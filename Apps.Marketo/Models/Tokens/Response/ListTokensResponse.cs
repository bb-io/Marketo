using Apps.Marketo.Dtos;

namespace Apps.Marketo.Models.Tokens.Response;

public class ListTokensResponse
{
    public IEnumerable<TokenDto> Tokens { get; set; }
}