using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo.Models.Forms.Responses
{
    public class ListFormFieldsResponse
    {
        [Display("Form fields")]
        public List<string> FormFieldsIds { get; set; }
    }
}
