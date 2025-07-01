using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Marketo.Dtos;

public class EmailDto
{
    [Display("Created at")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreatedAt { get; set; }
    public AssetFolder Folder { get; set; }

    [Display("From email")]
    public EmailHeaderField FromEmail { get; set; }

    [Display("From name")]
    public EmailHeaderField FromName { get; set; }


    [Display("Email ID")]
    public string Id { get; set; }
    public string Name { get; set; }
    public bool Operational { get; set; }

    [Display("Published to MSI")]
    public bool PublishToMSI { get; set; }

    [Display("Reply email")]
    public EmailHeaderField ReplyEmail { get; set; }
    public string Status { get; set; }
    public EmailHeaderField Subject { get; set; }

    
    public string Template { get; set; }

    [Display("Text only")]
    public bool TextOnly { get; set; }

    [Display("Updated at")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime UpdatedAt { get; set; }
    public string Url { get; set; }
    public int Version { get; set; }

    [Display("Web view")]
    public bool WebView { get; set; }
    public string Workspace { get; set; }

    [Display("Auto copy to text")]
    public bool AutoCopyToText { get; set; }

    [Display("Pre-header")]
    public string PreHeader { get; set; }

    [Display("CC fields")]
    public List<EmailCCField> CCFields { get; set; }
}

public class EmailHeaderField
{
    public string Type { get; set; }

    public string Value { get; set; }
}

public class EmailCCField
{
    [Display("Atrbute ID")]
    public string AttributeId { get; set; }

    [Display("Object name")]
    public string ObjectName { get; set; }

    [Display("Display name")]
    public string DisplayName { get; set; }

    [Display("API name")]
    public string ApiName { get; set; }
}

public class AssetFolder
{
    public string Type { get; set; }
    
    [Display("Id")]
    public string Value { get; set; }

    [Display("Folder name")]
    public string FolderName { get; set; }
}