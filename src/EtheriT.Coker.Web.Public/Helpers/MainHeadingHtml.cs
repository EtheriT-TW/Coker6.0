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
            var normalizedContent = ConvertEmptyMainHeadingsToDiv(
                htmlProcessor,
                contentHtml);
            var normalizedParent = ConvertAllMainHeadingsToDiv(
                htmlProcessor,
                parentHtml);

            return htmlProcessor.ComposeInheritedHtml(
                normalizedContent,
                normalizedParent);
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

        private static string ConvertEmptyMainHeadingsToDiv(
            IHtmlProcessor htmlProcessor,
            string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var document = htmlProcessor.LoadHtml(html);
            foreach (var heading in htmlProcessor.Find(document, "h1").Where(e => !HasText(e)))
            {
                heading.Name = "div";
            }

            return document.DocumentNode.OuterHtml;
        }

        private static string ConvertAllMainHeadingsToDiv(
            IHtmlProcessor htmlProcessor,
            string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var document = htmlProcessor.LoadHtml(html);
            foreach (var heading in htmlProcessor.Find(document, "h1"))
            {
                heading.Name = "div";
            }

            return document.DocumentNode.OuterHtml;
        }

        private static bool HasText(HtmlAgilityPack.HtmlNode heading)
        {
            return !string.IsNullOrWhiteSpace(
                WebUtility.HtmlDecode(heading.InnerText));
        }
    }
}
