using Apps.Marketo.Dtos;

namespace Apps.Marketo.Models.Folder.Responses
{
    public class ListFoldersResponse
    {
        public IEnumerable<FolderInfoDto> Folders { get; set; }
    }
}
