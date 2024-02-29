using System.Text.Json.Serialization;

namespace Apps.Marketo.Dtos;

public class LandingPageContentDto
{
    public string Id { get; set; }
    public string Type { get; set; }
    public int Index { get; set; }
    public object Content { get; set; }
}