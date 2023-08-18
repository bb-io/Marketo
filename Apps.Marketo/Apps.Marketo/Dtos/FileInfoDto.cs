using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Apps.Marketo.Dtos
{
    public class FileInfoDto
    {
        public string CreatedAt { get; set; }
        public string Description { get; set; }

        [JsonConverter(typeof(StringConverter))]
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
        [JsonConverter(typeof(StringConverter))]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
