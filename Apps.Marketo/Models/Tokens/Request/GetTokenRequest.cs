using Apps.Marketo.Models.Folder.Requests;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Tokens.Request;
public class GetTokenRequest : GetFolderInfoRequest
{
    [Display("Token name")]
    public string TokenName { get; set; }
}

