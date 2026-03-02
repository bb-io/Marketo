using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Marketo.Models.Content.Response;

public record DownloadContentResponse(FileReference Content) : IDownloadContentOutput
{
    [Display("Content")]
    public FileReference Content { get; set; } = Content;
}
