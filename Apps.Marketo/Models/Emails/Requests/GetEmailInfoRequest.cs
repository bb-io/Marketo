using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Emails.Requests;

public class GetEmailInfoRequest
{
    [Display("Email ID")]
    [DataSource(typeof(EmailDataHandler))]
    public string EmailId { get; set; }
}