using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.Models.Tokens.Request
{
    public class ListTokensRequest
    {
        [DataSource(typeof(TokenFolderDataHandler))]
        public string FolderId { get; set; }
    }
}
