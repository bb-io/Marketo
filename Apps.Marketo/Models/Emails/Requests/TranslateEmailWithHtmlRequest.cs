using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Marketo.Models.Emails.Requests
{
    public class TranslateEmailWithHtmlRequest
    {
        [Display("Translated HTML file")]
        public FileReference File {  get; set; }

        [Display("Translate only dynamic content")]
        public bool? TranslateOnlyDynamic { get; set; }

        [Display("Recreate corrupted modules", Description = "Marketo sometimes gives random \"System error\" on updating sections with dynamic content.\n" +
        "Blackbird will automatically try to reacreate such module from email template, then copy content from old corrupted module and then translate text content from HTML as usual.\n" +
        "Please be carefull, the ids of content section and the module itself will be changed! You can disable this feature by setting this parameter to false (default value is true).\n" +
        "You will see the list of recreated modules in action output. If list is empty - no corrupted modules were found.")]
        public bool? RecreateCorruptedModules { get; set; }
    }
}
