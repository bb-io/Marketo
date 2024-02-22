using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Marketo.Models.Files.Requests;

public class UpdateFileRequest : GetFileInfoRequest
{
    public FileReference File { get; set; }
}