using Apps.Marketo.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Files.Responses
{
    public class ListFilesResponse
    {
        public IEnumerable<FileInfoDto> Files { get; set; }
    }
}
