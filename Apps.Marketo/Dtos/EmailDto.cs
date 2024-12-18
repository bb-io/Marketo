﻿using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Marketo.Dtos;

public class EmailDto
{
    [Display("Created at")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreatedAt { get; set; }
    public AssetFolder Folder { get; set; }
    public EmailHeaderField FromEmail { get; set; }
    public EmailHeaderField FromName { get; set; }

    
    public string Id { get; set; }
    public string Name { get; set; }
    public bool Operational { get; set; }
    public bool PublishToMSI { get; set; }
    public EmailHeaderField ReplyEmail { get; set; }
    public string Status { get; set; }
    public EmailHeaderField Subject { get; set; }

    
    public string Template { get; set; }
    public bool TextOnly { get; set; }

    [Display("Updated at")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime UpdatedAt { get; set; }
    public string Url { get; set; }
    public int Version { get; set; }
    public bool WebView { get; set; }
    public string Workspace { get; set; }
    public bool AutoCopyToText { get; set; }
    public string PreHeader { get; set; }
    public List<EmailCCField> CCFields { get; set; }
}

public class EmailHeaderField
{
    public string Type { get; set; }

    public string Value { get; set; }
}

public class EmailCCField
{
    public string AttributeId { get; set; }
    public string ObjectName { get; set; }
    public string DisplayName { get; set; }
    public string ApiName { get; set; }
}

public class AssetFolder
{
    public string Type { get; set; }
    
    [Display("Id")]
    public string Value { get; set; }
    public string FolderName { get; set; }
}