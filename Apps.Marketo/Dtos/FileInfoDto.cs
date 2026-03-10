
using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Marketo.Dtos;

public class FileInfoDto
{
    [JsonConverter(typeof(DateTimeConverter))]
    [Display("Created at")]
    public DateTime CreatedAt { get; set; }
    public string Description { get; set; }

    [Display("File ID")]
    public string Id { get; set; }

    [Display("Mime type")]
    public string MimeType { get; set; }
    public string Name { get; set; }
    public int Size { get; set; }

    [Display("Updated at")]
    public string UpdatedAt { get; set; }
    public string Url { get; set; }
    public FileFolder Folder { get; set; }
}

public class FileFolder
{
    [Display("Folder ID")]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
}