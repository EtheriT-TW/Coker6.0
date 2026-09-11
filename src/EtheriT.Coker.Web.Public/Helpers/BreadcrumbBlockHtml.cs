using EtheriT.Coker.Application.Shared.Processor;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.Public.Helpers
{
    /// <summary>
    /// 內容區「麵包屑元件」的偵測與注入。
    /// 後台只會存下空殼 div，實際 markup 在前台輸出時才由伺服器填入。
    /// </summary>
    internal static class BreadcrumbBlockHtml
    {
        private const string BlockSelector = "div[data-coker-block='breadcrumb']";

        // 屬性值的引號樣式會因序列化而異，用 Regex 比字串比對可靠。
        private static readonly Regex BlockPattern = new(
            @"data-coker-block\s*=\s*[""']?breadcrumb[""']?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool Exists(string? html)
            => !string.IsNullOrWhiteSpace(html) && BlockPattern.IsMatch(html);

        public static string Inject(
            IHtmlProcessor htmlProcessor,
            string html,
            string? breadcrumbHtml)
        {
            // 沒有元件就完全不解析 DOM，避免每頁多一次 HTML parse。
            if (!Exists(html))
            {
                return html;
            }

            var document = htmlProcessor.LoadHtml(html);
            var filled = false;

            foreach (var slot in htmlProcessor.Find(document, BlockSelector))
            {
                if (!filled && !string.IsNullOrWhiteSpace(breadcrumbHtml))
                {
                    slot.InnerHtml = breadcrumbHtml;
                    filled = true;
                }
                else
                {
                    // 第二個以後的元件一律移除，避免重複麵包屑；
                    // breadcrumbHtml 為空（例：ShowPagePath 關閉）時也一併移除。
                    slot.Remove();
                }
            }

            return document.DocumentNode.OuterHtml;
        }
    }
}