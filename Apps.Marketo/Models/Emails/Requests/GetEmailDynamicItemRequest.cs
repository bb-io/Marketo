using Apps.Marketo.DataSourceHandlers.Deprecated;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Emails.Requests
{
    public class GetEmailDynamicItemRequest
    {
        [Display("Dynamic content ID")]
        [DataSource(typeof(EmailDynamicItemsDataHandler))]
        public string DynamicContentId { get; set; }
    }
}
