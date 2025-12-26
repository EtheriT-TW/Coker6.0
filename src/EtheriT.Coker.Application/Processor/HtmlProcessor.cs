using EtheriT.Coker.Application.Shared.Processor;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EtheriT.Coker.Application.Processor
{
    public class HtmlProcessor : IHtmlProcessor
    {
        public string RemoveNode(string html, string selector)
        {
            if (!string.IsNullOrEmpty(html) && !string.IsNullOrEmpty(selector))
            {
                HtmlDocument doc = LoadHtml(html);
                var nodes = Find(doc, selector);
                foreach (var node in nodes)
                {
                    node.Remove();
                }
                return doc.DocumentNode.OuterHtml;
            }
            else return html;
        }
        public string SetAttr(string html, string selector, string attrName, string attrValue)
        {
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(selector) || string.IsNullOrEmpty(attrName))
                return html;

            var doc = LoadHtml(html);
            var nodes = Find(doc, selector);
            foreach (var node in nodes)
            {
                node.SetAttributeValue(attrName, attrValue);
            }
            return doc.DocumentNode.OuterHtml;
        }
        public List<string> find(string html, string selector)
        {
            List<string> nodesStr = new List<string>();
            HtmlDocument doc = LoadHtml(html);
            var nodes = Find(doc, selector);
            foreach (var node in nodes)
            {
                nodesStr.Add(node.OuterHtml);
            }
            return nodesStr;
        }
        public string text(string html)
        {
            HtmlDocument doc = LoadHtml(RemoveNode(html, ".material-symbols-outlined"));
            // 取得去除標籤後的純文字
            string innerText = doc.DocumentNode.InnerText;

            // 使用正則表達式過濾掉多餘的空白字元和換行符
            string cleanedText = Regex.Replace(innerText, @"\s+", " ").Trim();

            return cleanedText;
        }
        public string ExtractStyleCss(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            var doc = LoadHtml(html);
            var styleNodes = Find(doc, "style");
            return string.Join(Environment.NewLine, styleNodes.Select(n => n.InnerHtml?.Trim() ?? ""));
        }

        public HtmlDocument LoadHtml(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            return doc;
        }
        public List<HtmlNode> Find(HtmlDocument document, string selector)
        {
            var Nodes = new List<HtmlNode>();
            if (string.IsNullOrEmpty(selector)) return Nodes;
            else if (selector.Contains(","))
            {
                var selectors = selector.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));
                foreach (var sel in selectors)
                {
                    Nodes.AddRange(Find(document, sel));
                }
            }
            else
            {
                string xpath = CssSelectorToXPath(selector);
                try
                {
                    var matched = document.DocumentNode.SelectNodes(xpath);
                    if (matched == null || matched.Count == 0)
                    {
                        Console.WriteLine($"xpath:{xpath},select:{selector}");
                    }
                    return matched?.ToList() ?? Nodes;
                }
                catch (Exception ex) // 可改用 XPathException
                {
                    // 紀錄以便快速定位
                    Console.WriteLine($"XPath error: {ex.Message}\nXPath: {xpath}\nSelector: {selector}");
                }
            }
            return Nodes;
        }
        public string ExtractBodyInnerHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            var doc = LoadHtml(html);

            // 優先用 XPath 找 body
            var body = doc.DocumentNode.SelectSingleNode("//body");
            if (body != null)
            {
                return body.InnerHtml ?? string.Empty;
            }

            // 沒有 body：代表本來就是片段，直接回傳原字串
            return html;
        }
        // 將 CSS 選擇器轉換為 XPath

        private string CssSelectorToXPath(string cssSelector)
        {
            if (string.IsNullOrWhiteSpace(cssSelector)) return "//";

            var s = cssSelector.Trim();

            // 1) 暫存引號內容，避免把引號內空白誤當成後代分隔
            var quoted = new List<string>();
            s = Regex.Replace(s, @"(['""])(?:\\.|(?!\1).)*\1", m =>
            {
                var full = m.Value;                       // e.g.  '_blank'
                var inner = full.Substring(1, full.Length - 2); //  _blank
                quoted.Add(inner);
                return $"##Q{quoted.Count - 1}##";
            });

            // 名稱規則（元素/屬性）
            const string Tag = @"([A-Za-z_][A-Za-z0-9_\-:]*)";
            const string Attr = @"([A-Za-z_][A-Za-z0-9_\-:\.]*)";

            // 2) Combinators
            s = Regex.Replace(s, @"\s*>\s*", "/");                     // 子代
            s = Regex.Replace(s, @"\s*\+\s*", "##ADJ##");              // 相鄰兄弟（先標記，稍後還原）
            s = Regex.Replace(s, @"\s*~\s*", "/following-sibling::");  // 一般兄弟
            s = Regex.Replace(s, @"\s+", "##DS##");                    // 後代（空白）

            // 3) #id
            s = Regex.Replace(s, $@"{Tag}#([\w\-]+)", "$1[@id='$2']");
            s = Regex.Replace(s, @"(^|//|/|##DS##)#([\w\-]+)", "$1*[@id='$2']");

            // 4) .class（含多 class 連寫）
            while (Regex.IsMatch(s, $@"{Tag}\.([\w\-]+)"))
                s = Regex.Replace(s, $@"{Tag}\.([\w\-]+)",
                    "$1[contains(concat(' ', normalize-space(@class), ' '), ' $2 ')]");

            while (Regex.IsMatch(s, @"(^|//|/|##DS##)\.([\w\-]+)"))
                s = Regex.Replace(s, @"(^|//|/|##DS##)\.([\w\-]+)",
                    "$1*[contains(concat(' ', normalize-space(@class), ' '), ' $2 ')]");

            // 5) 屬性選擇器
            s = Regex.Replace(s, $@"\[{Attr}=['""]?([^'""]+)['""]?\]", @"[@$1='$2']");
            s = Regex.Replace(s, $@"\[{Attr}\^=['""]?([^'""]+)['""]?\]", @"[starts-with(@$1,'$2')]");
            s = Regex.Replace(s, $@"\[{Attr}\$=['""]?([^'""]+)['""]?\]",
                @"[substring(@$1, string-length(@$1)-string-length('$2')+1)='$2']");
            s = Regex.Replace(s, $@"\[{Attr}\*=['""]?([^'""]+)['""]?\]", @"[contains(@$1,'$2')]");
            s = Regex.Replace(s, $@"\[{Attr}\]", @"[@$1]");

            // 純屬性起點補 *（片段起點或分隔符後直接 "["）
            s = Regex.Replace(s, @"(^|//|/|##DS##)\[", "$1*[");

            // 6) 還原相鄰兄弟：
            //   E + F → E/following-sibling::*[1][self::F]（F 為單純元素名時）
            s = Regex.Replace(s, $@"##ADJ##{Tag}", @"/following-sibling::*[1][self::$1]");
            //   其他情況（如 + .a 或 + [attr]）退化為「下一個兄弟」再接條件
            s = s.Replace("##ADJ##", "/following-sibling::*[1]");

            // 7) 還原後代與引號
            s = s.Replace("##DS##", "//");
            for (int i = 0; i < quoted.Count; i++)
            {
                // 只還原內文（不帶引號），因為等號替換時已經加上了引號
                s = s.Replace($"##Q{i}##", quoted[i]);
            }

            // 8) 清理與保險
            s = Regex.Replace(s, @"//+", "//");
            s = s.Replace("//[", "//*[");
            s = s.Replace("/[", "/*[");

            if (!s.StartsWith("//"))
                s = "//" + s.TrimStart('/');

            return s;
        }

    }
}
