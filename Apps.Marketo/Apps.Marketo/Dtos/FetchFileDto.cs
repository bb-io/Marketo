using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Dtos
{
    public class FetchFileDto
    {
        public List<Error> Errors { get; set; }
        public string RequestId { get; set; }
        public List<FileResponse> Result { get; set; }
        public bool Success { get; set; }
        public List<string> Warnings { get; set; }
    }

    public class FileResponse
    {
        public string CreatedAt { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public string MimeType { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public string UpdatedAt { get; set; }
        public string Url { get; set; }
    }

    public class Error
    {
        public string Message { get; set; }
        public string Code { get; set; }
    }
}
