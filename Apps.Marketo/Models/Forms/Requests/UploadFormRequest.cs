using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Marketo.Models.Forms.Requests;

public class UploadFormRequest
{
    [Display("Content")]
    public FileReference File { get; set; }
}
