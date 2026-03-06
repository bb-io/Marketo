using HtmlAgilityPack;
using Apps.Marketo.Models.Entities;
using Apps.Marketo.Models.Utility.Html;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Marketo.HtmlHelpers;

public static class HtmlContentBuilder
{
    private const string HtmlIdAttribute = "id";
    
    public static string GenerateHtml(
        Dictionary<string, string> sections, 
        string title,
        List<MetadataEntity> metadataEntities)
    {
        var htmlDoc = new HtmlDocument();
        var htmlNode = htmlDoc.CreateElement("html");
        htmlDoc.DocumentNode.AppendChild(htmlNode);

        var headNode = htmlDoc.CreateElement("head");
        htmlNode.AppendChild(headNode);

        foreach (var meta in metadataEntities)
        {
            if (!string.IsNullOrWhiteSpace(meta.MetadataContent))
            {
                var metaNode = htmlDoc.CreateElement("meta");
                metaNode.SetAttributeValue("name", meta.MetadataName);
                metaNode.SetAttributeValue("content", meta.MetadataContent);
                headNode.AppendChild(metaNode);
            }
        }

        var titleNode = htmlDoc.CreateElement("title");
        headNode.AppendChild(titleNode);
        titleNode.InnerHtml = title;

        var bodyNode = htmlDoc.CreateElement("body");
        htmlNode.AppendChild(bodyNode);

        foreach (var section in sections)
        {
            if (!string.IsNullOrWhiteSpace(section.Value))
            {
                var sectionNode = htmlDoc.CreateElement("div");
                sectionNode.SetAttributeValue(HtmlIdAttribute, section.Key);
                sectionNode.InnerHtml = section.Value;
                bodyNode.AppendChild(sectionNode);
            }
        }
        return htmlDoc.DocumentNode.OuterHtml;
    }

    public static Dictionary<string, string> ParseHtml(string html)
    {
        var result = new Dictionary<string, string>();
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
        var body = htmlDoc.DocumentNode.SelectSingleNode("//body") ?? 
            throw new PluginMisconfigurationException("HTML does not have a <body> tag");

        foreach (var section in body.ChildNodes)
        {
            if (section.NodeType == HtmlNodeType.Element && section.Attributes.Contains(HtmlIdAttribute))
                result.Add(section.Attributes[HtmlIdAttribute].Value, section.InnerHtml);
        }
        
        return result;
    }

    public static List<MetaTag> ExtractAllMetaTags(string html)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var metaNodes = htmlDoc.DocumentNode.SelectNodes("//meta[@name and @content]");
        var metaTags = new List<MetaTag>();

        if (metaNodes != null)
        {
            foreach (var node in metaNodes)
            {
                var name = node.GetAttributeValue("name", string.Empty);
                var content = node.GetAttributeValue("content", string.Empty);

                if (!string.IsNullOrWhiteSpace(name))
                    metaTags.Add(new MetaTag(name, content));
            }
        }

        return metaTags;
    }

    public static string GetRequiredMetaValue(
        string? inputValue, 
        List<MetaTag> metaTags, 
        string metaKey, 
        string friendlyName)
    {
        if (!string.IsNullOrWhiteSpace(inputValue))
            return inputValue;

        var meta = metaTags.FirstOrDefault(m => string.Equals(m.Name, metaKey, StringComparison.OrdinalIgnoreCase));
        if (meta != null && !string.IsNullOrWhiteSpace(meta.Content))
            return meta.Content;

        throw new PluginMisconfigurationException(
            $"{friendlyName} was not found in the input file. Please provide it in the optional input.");
    }
}
