using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.EmailTemplates.Requests
{
    public class ListEmailTemplatesRequest
    {
        [DataSource(typeof(StatusDataHandler))]
        public string? Status { get; set; }
    }
}
