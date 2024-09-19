using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.EmailTemplates.Requests
{
    public class GetEmailTemplateRequest
    {
        [Display("Email template ID")]
        [DataSource(typeof(EmailTemplateDataHandler))]
        public string EmailTemplateId { get; set; }
    }
}
