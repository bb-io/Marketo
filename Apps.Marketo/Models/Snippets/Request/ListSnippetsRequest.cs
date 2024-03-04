using Apps.Marketo.DataSourceHandlers;
using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Snippets.Request
{
    public class ListSnippetsRequest
    {
        [DataSource(typeof(StatusDataHandler))]
        public string? Status { get; set; }

        [Display("Folder", Description = "Folders list with \"Snippet\" type")]
        [DataSource(typeof(SnippetFolderDataHandler))]
        public string? FolderId { get; set; }

        [Display("Earliest updated at")]
        public DateTime? EarliestUpdatedAt { get; set; }

        [Display("Latest updated at")]
        public DateTime? LatestUpdatedAt { get; set; }
    }
}
