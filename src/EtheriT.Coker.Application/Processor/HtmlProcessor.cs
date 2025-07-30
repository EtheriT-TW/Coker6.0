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
	public class HtmlProcessor: IHtmlProcessor
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
        public List<string> find(string html, string selector) {
			List<string> nodesStr = new List<string>();
			HtmlDocument doc = LoadHtml(html);
			var nodes = Find(doc, selector);
			foreach (var node in nodes) {
				nodesStr.Add(node.OuterHtml);
			}
			return nodesStr;
		}
		public string text(string html) {
			HtmlDocument doc = LoadHtml(RemoveNode(html, ".material-symbols-outlined"));
			// 取得去除標籤後的純文字
			string innerText = doc.DocumentNode.InnerText;

			// 使用正則表達式過濾掉多餘的空白字元和換行符
			string cleanedText = Regex.Replace(innerText, @"\s+", " ").Trim();

			return cleanedText;
		}

		public HtmlDocument LoadHtml(string htmlContent)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(htmlContent);
			return doc;
		}
		public List<HtmlNode> Find(HtmlDocument document, string selector)
		{
			if (string.IsNullOrEmpty(selector)) return new List<HtmlNode>();
			string xpath = CssSelectorToXPath(selector);
			return document.DocumentNode.SelectNodes(xpath)?.ToList() ?? new List<HtmlNode>();
		}
        // 將 CSS 選擇器轉換為 XPath
        private string CssSelectorToXPath(string cssSelector)
        {
            // 嚴格子節點（>）轉換為 /
            cssSelector = cssSelector.Replace(">", "/");

            // 相鄰兄弟（+）
            cssSelector = cssSelector.Replace("+", "/following-sibling::*[1]");

            // 一般兄弟（~）
            cssSelector = cssSelector.Replace("~", "/following-sibling::");

            // 處理空白為後代（避免與 >、+、~ 混淆）
            cssSelector = Regex.Replace(cssSelector, @"\s+(?![>+~])", "##DOUBLE_SLASH##");

            // ID (#id)
            cssSelector = Regex.Replace(cssSelector, @"#([\w-]+)", "[@id='$1']");

            // class（保留元素型別）
            cssSelector = Regex.Replace(cssSelector, @"(\w*)\.([\w-]+)", "$1[contains(concat(' ', normalize-space(@class), ' '), ' $2 ')]");

            // 精確屬性 [attr="value"]
            cssSelector = Regex.Replace(cssSelector, @"\[(\w+)=['""]?([^'""]+)['""]?\]", "[@$1='$2']");

            // 存在屬性 [attr]
            cssSelector = Regex.Replace(cssSelector, @"\[(\w+)\]", "[@$1]");

            // 萬用元素 *
            cssSelector = Regex.Replace(cssSelector, @"(^|\W)\*", "$1*");

            // 去除多餘斜線
            cssSelector = Regex.Replace(cssSelector, "/{2,}", "/");

            // 還原 //
            cssSelector = cssSelector.Replace("##DOUBLE_SLASH##", "//");

            // 確保 XPath 是以 // 開頭
            if (!cssSelector.StartsWith("//"))
            {
                cssSelector = "//" + cssSelector.TrimStart('/');
            }

            return cssSelector;
        }
    }
}
