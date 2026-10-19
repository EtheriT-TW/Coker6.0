using EtheriT.Coker.Application.Shared.Processor;
using System.Net;

namespace EtheriT.Coker.Web.Public.Helpers
{
    internal static class MainHeadingHtml
    {
        public static string PrepareInheritedContent(
            IHtmlProcessor htmlProcessor,
            string? contentHtml,
            string? parentHtml)
        {
            return PrepareWithHeadingCount(htmlProcessor, contentHtml, parentHtml).Html;
        }

        // 冷建置時子內容／父層各解析一次，同一份 DOM 完成合併與 H1 統計。
        public static (string Html, int MainHeadingCount) PrepareWithHeadingCount(
            IHtmlProcessor htmlProcessor, string? contentHtml, string? parentHtml)
        {
            var prepared = PrepareDocument(htmlProcessor, contentHtml, parentHtml);
            var root = prepared.BodyOnly
                ? prepared.Document.DocumentNode.SelectSingleNode("//body")! : prepared.Document.DocumentNode;
            return (prepared.BodyOnly ? root.InnerHtml : root.OuterHtml, root.Descendants("h1").Count(HasText));
        }

        public static (HtmlAgilityPack.HtmlDocument Document, bool BodyOnly) PrepareDocument(
            IHtmlProcessor htmlProcessor, string? contentHtml, string? parentHtml)
        {
            var content = htmlProcessor.LoadHtml(contentHtml ?? "");
            foreach (var heading in htmlProcessor.Find(content, "h1").Where(e => !HasText(e)))
                heading.Name = "div";
            if (string.IsNullOrWhiteSpace(parentHtml))
                return (content, false);

            var parent = htmlProcessor.LoadHtml(parentHtml);
            foreach (var heading in htmlProcessor.Find(parent, "h1")) heading.Name = "div";
            foreach (var node in htmlProcessor.Find(parent, ".catalog_frame,.noInherit").ToList()) node.Remove();
            var slot = htmlProcessor.Find(parent, ".subpage_content").FirstOrDefault();
            if (slot?.ParentNode == null)
                return (content, false);
            foreach (var node in content.DocumentNode.ChildNodes.ToList()) slot.ParentNode.InsertBefore(node, slot);
            slot.Remove();
            var body = parent.DocumentNode.SelectSingleNode("//body");
            return (parent, body != null);
        }

        public static int CountMainHeadings(
            IHtmlProcessor htmlProcessor,
            string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return 0;
            }

            var document = htmlProcessor.LoadHtml(html);
            return htmlProcessor.Find(document, "h1").Count(HasText);
        }


        public static bool HasText(HtmlAgilityPack.HtmlNode heading)
        {
            return !string.IsNullOrWhiteSpace(
                WebUtility.HtmlDecode(heading.InnerText));
        }
    }
}
