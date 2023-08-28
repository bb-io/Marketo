using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Marketo.Models.Files.Requests
{
    public class UploadFileRequest
    {
        public string? Description { get; set; }

        public File File { get; set; }

        [Display("Insert only")]
        public bool? InsertOnly { get; set; }

        [Display("Folder")]
        [DataSource(typeof(FolderDataHandler))]
        public string FolderId { get; set; }

        [Display("Folder type")]
        [DataSource(typeof(FolderTypeDataHandler))]
        public string Type { get; set; }
    }
}
