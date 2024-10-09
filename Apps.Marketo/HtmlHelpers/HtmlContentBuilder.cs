using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using HtmlAgilityPack;
using System.Text;
using Apps.Marketo.Models.Entities;

namespace Apps.Marketo.HtmlHelpers
{
    public static class HtmlContentBuilder
    {
        private const string HtmlIdAttribute = "id";
        
        public static string GenerateHtml(Dictionary<string, string> sections, string title, string language, HtmlIdEntity entity)
        {
            var htmlDoc = new HtmlDocument();
            var htmlNode = htmlDoc.CreateElement("html");
            htmlDoc.DocumentNode.AppendChild(htmlNode);

            var headNode = htmlDoc.CreateElement("head");
            htmlNode.AppendChild(headNode);
            
            var metaNode = htmlDoc.CreateElement("meta");
            metaNode.SetAttributeValue("name", entity.MetadataName);
            metaNode.SetAttributeValue("content", entity.Id);
            headNode.AppendChild(metaNode);

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
            var sections = htmlDoc.DocumentNode.SelectSingleNode("//body").ChildNodes;
            foreach (var section in sections)
            {
                result.Add(section.Attributes[HtmlIdAttribute].Value, section.InnerHtml);
            }
            
            return result;
        }
        
        public static string? ExtractIdFromMeta(string html, string metadataName)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var metaNode = htmlDoc.DocumentNode.SelectSingleNode($"//meta[@name='{metadataName}']");
            return metaNode?.Attributes["content"].Value;
        }
    }
}
