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
    public class GetFolderInfoRequest
    {
        [Display("Folder")]
        [DataSource(typeof(FolderDataHandler))]
        public string FolderId { get; set; }
    }
}
