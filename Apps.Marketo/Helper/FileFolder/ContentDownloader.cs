using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Transformations;
using Blackbird.Filters.Xliff.Xliff2;
using System.Text;

namespace Apps.Marketo.Helper.FileFolder;

public static class ContentDownloader
{
    public static async Task<string> DownloadHtmlContent(
        IFileManagementClient fileManagementClient, 
        FileReference fileContent)
    {
        var file = await fileManagementClient.DownloadAsync(fileContent);
        var html = Encoding.UTF8.GetString(await file.GetByteData());

        if (Xliff2Serializer.IsXliff2(html))
        {
            html = Transformation.Parse(html, $"{fileContent.Name}.xlf").Target().Serialize() ??
                throw new PluginMisconfigurationException("XLIFF did not contain files");
        }

        return html;
    }
}
