using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Emails.Requests;

public class GetEmailInfoRequest
{
    [DataSource(typeof(EmailDataHandler))]
    public string EmailId { get; set; }
}