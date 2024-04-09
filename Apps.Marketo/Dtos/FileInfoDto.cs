
using Newtonsoft.Json;

namespace Apps.Marketo.Dtos;

public class FileInfoDto
{
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreatedAt { get; set; }
    public string Description { get; set; }
    public string Id { get; set; }
    public string MimeType { get; set; }
    public string Name { get; set; }
    public int Size { get; set; }
    public string UpdatedAt { get; set; }
    public string Url { get; set; }
    public FileFolder Folder { get; set; }
}

public class Error
{
    public string Message { get; set; }
    public string Code { get; set; }
}

public class FileFolder
{
    
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
}