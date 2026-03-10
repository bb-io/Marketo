using Apps.Marketo.DataSourceHandlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Marketo.Models.EmailTemplates.Requests;

public class ListEmailTemplatesRequest
{
    [Display("Status"), StaticDataSource(typeof(StatusDataHandler))]
    public string? Status { get; set; }
}
