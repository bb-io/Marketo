using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Tokens.Request;
public class GetTokenRequest
{
    [DataSource(typeof(TokenFolderDataHandler))]
    [Display("Folder ID")]
    public string FolderId { get; set; }

    [Display("Token name")]
    public string TokenName { get; set; }
}

