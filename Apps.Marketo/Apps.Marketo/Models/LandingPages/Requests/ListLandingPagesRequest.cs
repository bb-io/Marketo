using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.LandingPages.Requests
{
    public class ListLandingPagesRequest
    {
        [DataSource(typeof(StatusDataHandler))]
        public string? Status { get; set; }

        [Display("Max return (maximum 200)")]
        public int? MaxReturn { get; set; }
        public int? Offset { get; set; }

        [Display("Folder")]
        [DataSource(typeof(FolderDataHandler))]
        public string? FolderId { get; set; }

        [Display("Folder type")]
        [DataSource(typeof(FolderTypeDataHandler))]
        public string? Type { get; set; }
    }
}
