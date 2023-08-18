using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Folder.Requests
{
    public class CreateFolderRequest
    {
        public string Description { get; set; }
        public string Name { get; set; }

        [Display("Parent folder")]
        [DataSource(typeof(FolderDataHandler))]
        public string FolderId { get; set; }

        [Display("Parent folder type")]
        [DataSource(typeof(FolderTypeDataHandler))]
        public string? Type { get; set; }
    }
}
