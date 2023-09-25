using Apps.Marketo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Marketo.Models.Files.Requests
{
    public class GetFileInfoRequest
    {
        [Display("File")]
        [DataSource(typeof(FileDataHandler))]
        public string FileId { get; set; }
    }
}
