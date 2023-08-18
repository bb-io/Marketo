using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Apps.Marketo.DataSourceHandlers;
using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Files.Requests
{
    public class UploadFileRequest
    {
        public string? Description { get; set; }

        public byte[] File { get; set; }

        [Display("Insert only")]
        public bool? InsertOnly { get; set; }

        public string Name { get; set; }

        [Display("Folder")]
        [DataSource(typeof(FolderDataHandler))]
        public string FolderId { get; set; }

        [Display("Folder type")]
        [DataSource(typeof(FolderTypeDataHandler))]
        public string Type { get; set; }
    }
}
