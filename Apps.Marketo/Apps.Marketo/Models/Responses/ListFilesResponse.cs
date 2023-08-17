using Apps.Marketo.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Responses
{
    public class ListFilesResponse
    {
        public IEnumerable<FileResponse> Files { get; set; }
    }
}
