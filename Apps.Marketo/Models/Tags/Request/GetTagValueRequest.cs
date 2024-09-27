using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Tags.Request
{
    public class GetTagValueRequest
    {
        [Display("Tag value")]
        [DataSource(typeof(TagValueDataHandler))]
        public string TagValue { get; set; }
    }
}
