using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Marketo.Models.Files.Requests;

public class UpdateFileRequest : GetFileInfoRequest
{
    public File File { get; set; }
}