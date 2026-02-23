using Apps.Marketo.DataSourceHandlers;
using Apps.Marketo.DataSourceHandlers.FolderDataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Tokens.Request;

public class CreateTokenRequest
{
    public string Name { get; set; }
    
    [DataSource(typeof(TokenTypeDataHandler))]
    public string Type { get; set; }
    
    public string Value { get; set; }

    [DataSource(typeof(TokenFolderDataHandler))]
    [Display("Folder ID")]
    public string FolderId { get; set; }
}