using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Emails.Responses;

public class TranslateEmailWithHtmlResponse
{
    [Display("Recreated modules from scratch", Description = "Marketo sometimes gives random \"System error\" on updating sections with dynamic content.\n" +
        "Blackbird will automatically try to reacreate such module from email template, then copy content from corrupted module and then translate text content from HTML as usual.\n" +
        "Please be carefull the ids of content section and the module itself will be changed! You can disable this feature in optional input parameters of this action (\"Reacreate corrupted modules\" set to false)")]
    public List<string> RecreateModules { get; set; }
}
