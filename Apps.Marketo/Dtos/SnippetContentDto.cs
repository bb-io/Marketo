namespace Apps.Marketo.Dtos;

public class SnippetContentDto
{
    public SnippetContentDto(string type, string content)
    {
        Type = type;
        Content = content;
    }

    public string Type { get; set; }
    
    public string Content { get; set; }
}