using Newtonsoft.Json;
using Apps.Marketo.Helper.Interfaces;

namespace Apps.Marketo.Models.Entities.Email;

public class EmailEntity : IEntityName, IEntityFolder, IEntityCreatedAt
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("createdAt"), JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt"), JsonConverter(typeof(DateTimeConverter))]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("folder")]
    public AssetFolder Folder { get; set; }

    [JsonProperty("fromEmail")]
    public EmailHeaderField FromEmail { get; set; }

    [JsonProperty("fromName")]
    public EmailHeaderField FromName { get; set; }

    [JsonProperty("operational")]
    public bool Operational { get; set; }

    [JsonProperty("publishToMSI")]
    public bool PublishToMSI { get; set; }

    [JsonProperty("replyEmail")]
    public EmailHeaderField ReplyEmail { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("subject")]
    public EmailHeaderField Subject { get; set; }

    [JsonProperty("template")]
    public string TemplateId { get; set; } = string.Empty;

    [JsonProperty("textOnly")]
    public bool TextOnly { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;

    [JsonProperty("version")]
    public int Version { get; set; }

    [JsonProperty("webView")]
    public bool WebView { get; set; }

    [JsonProperty("workspace")]
    public string WorkspaceName { get; set; } = string.Empty;

    [JsonProperty("autoCopyToText")]
    public bool AutoCopyToText { get; set; }

    [JsonProperty("preHeader")]
    public string PreHeader { get; set; } = string.Empty;

    [JsonProperty("ccFields")]
    public List<EmailCCField> CCFields { get; set; } = [];
}