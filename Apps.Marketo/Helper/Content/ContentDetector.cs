using Apps.Marketo.Constants;
using Apps.Marketo.HtmlHelpers;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Marketo.Helper.Content;

public static class ContentDetector
{
    public static string DetectContentType(string inputHtmlContent)
    {
        var metaTags = HtmlContentBuilder.ExtractAllMetaTags(inputHtmlContent);

        if (metaTags.Any(m => m.Name == MetadataConstants.BlackbirdEmailId))
            return ContentTypes.Email;

        if (metaTags.Any(m => m.Name == MetadataConstants.BlackbirdFormId))
            return ContentTypes.Form;

        if (metaTags.Any(m => m.Name == MetadataConstants.BlackbirdSnippetId))
            return ContentTypes.Snippet;

        if (metaTags.Any(m => m.Name == MetadataConstants.BlackbirdLandingPageId))
            return ContentTypes.LandingPage;

        throw new PluginMisconfigurationException("The content type cannot be recognized automatically");
    }
}
