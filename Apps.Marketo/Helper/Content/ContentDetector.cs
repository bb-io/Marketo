using Apps.Marketo.Constants;
using Apps.Marketo.HtmlHelpers;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Marketo.Helper.Content;

public static class ContentDetector
{
    public static string DetectContentType(string inputHtmlContent)
    {
        if (!string.IsNullOrEmpty(HtmlContentBuilder.ExtractMeta(inputHtmlContent, MetadataConstants.BlackbirdEmailId)))
            return ContentTypes.Email;

        if (!string.IsNullOrEmpty(HtmlContentBuilder.ExtractMeta(inputHtmlContent, MetadataConstants.BlackbirdFormId)))
            return ContentTypes.Form;

        if (!string.IsNullOrEmpty(HtmlContentBuilder.ExtractMeta(inputHtmlContent, MetadataConstants.BlackbirdSnippetId)))
            return ContentTypes.Snippet;

        if (!string.IsNullOrEmpty(HtmlContentBuilder.ExtractMeta(inputHtmlContent, MetadataConstants.BlackbirdLandingPageId)))
            return ContentTypes.LandingPage;

        throw new PluginMisconfigurationException("The content type cannot be recognized automatically");
    }
}
