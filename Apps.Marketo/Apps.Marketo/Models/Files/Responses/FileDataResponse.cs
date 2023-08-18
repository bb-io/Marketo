using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Files.Responses
{
    public class FileDataResponse
    {
        public string Filename { get; set; }

        public byte[] File { get; set; }
    }
}
