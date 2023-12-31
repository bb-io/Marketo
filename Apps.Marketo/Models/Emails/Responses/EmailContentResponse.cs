﻿using Apps.Marketo.Dtos;

namespace Apps.Marketo.Models.Emails.Responses;

public class EmailContentResponse
{
    public EmailContentResponse(EmailContentDto email)
    {
        ContentType = email.ContentType;
        HtmlId = email.HtmlId;
        Index = email.Index;
        IsLocked = email.IsLocked;
        ParentHtmlId = email.ParentHtmlId;
    }
    public string ContentType { get; set; }
    public string HtmlId { get; set; }
    public int Index { get; set; }
    public bool IsLocked { get; set; }
    public string ParentHtmlId { get; set; }
}