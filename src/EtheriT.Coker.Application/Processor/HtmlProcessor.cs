using EtheriT.Coker.Application.Shared.Processor;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
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
			string cleanedText = System.Text.RegularExpressions.Regex.Replace(innerText, @"\s+", " ").Trim();

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
			// 先處理子選擇器（>），它表示嚴格的子節點關係
			cssSelector = cssSelector.Replace(">", "/");

			// 處理相鄰兄弟選擇器（+）
			cssSelector = cssSelector.Replace("+", "/following-sibling::*[1]");

			// 處理一般兄弟選擇器（~）
			cssSelector = cssSelector.Replace("~", "/following-sibling::");

			// 處理後代選擇器（空格），避免與其他選擇器重疊
			cssSelector = System.Text.RegularExpressions.Regex.Replace(cssSelector, @"\s+(?![>+~])", "##DOUBLE_SLASH##");

			// 處理 ID 選擇器 (#id)
			cssSelector = System.Text.RegularExpressions.Regex.Replace(cssSelector, @"#([\w-]+)", "//*[@id='$1']");

			// 處理類選擇器 (.class)，保持元素類型
			cssSelector = System.Text.RegularExpressions.Regex.Replace(cssSelector, @"\.([\w-]+)", "//*[contains(concat(' ', normalize-space(@class), ' '), ' $1 ')]");

			// 處理屬性選擇器 [attr=value]
			cssSelector = System.Text.RegularExpressions.Regex.Replace(cssSelector, @"\[(\w+)=([\w'-]+)\]", "[@$1='$2']");

			// 避免多餘的斜杠連續出現（例如 "////"）
			cssSelector = System.Text.RegularExpressions.Regex.Replace(cssSelector, "/{2,}", "/");

			// 將特殊標記替換回 //
			cssSelector = cssSelector.Replace("##DOUBLE_SLASH##", "//");

			// 確保生成的 XPath 以 "//" 開頭
			if (cssSelector.StartsWith("/"))
			{
				cssSelector = "//" + cssSelector.TrimStart('/');
			}
			else
			{
				cssSelector = "//" + cssSelector; // 如果不是以 / 開頭，則直接添加 //
			}

			return cssSelector;
		}
	}
}
