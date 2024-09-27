using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Tags.Request
{
    public class GetTagTypeRequest
    {
        [Display("Tag type")]
        [DataSource(typeof(TagTypeDataHandler))]
        public string TagType { get; set; }
    }
}
