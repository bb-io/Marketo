using Apps.Marketo.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Folder.Responses
{
    public class ListFoldersResponse
    {
        public IEnumerable<FolderInfoDto> Folders { get; set; }
    }
}
