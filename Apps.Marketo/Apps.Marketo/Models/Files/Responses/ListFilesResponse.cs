using Apps.Marketo.Dtos;

namespace Apps.Marketo.Models.Files.Responses
{
    public class ListFilesResponse
    {
        public IEnumerable<FileInfoDto> Files { get; set; }
    }
}
