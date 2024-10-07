using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Forms.Requests
{
    public class ListFormFieldsRequest
    {
        [Display("Forms fields")]
        [DataSource(typeof(MultipleFormsFieldsDataHandler))]
        public List<string> FormFields { get; set; }
    }
}
