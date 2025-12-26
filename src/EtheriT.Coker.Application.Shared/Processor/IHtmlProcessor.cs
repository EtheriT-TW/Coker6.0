using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;

namespace EtheriT.Coker.Application.Shared.Processor
{
	public interface IHtmlProcessor
	{
		public string RemoveNode(string html,string selector);
		public string SetAttr(string html, string selector, string attrName, string attrValue);
        public List<string> find(string html, string selector);
		public List<HtmlNode> Find(HtmlDocument document, string selector);
		public HtmlDocument LoadHtml(string htmlContent);
		public string ExtractBodyInnerHtml(string html);
        public string text(string html);
		public string ExtractStyleCss(string html);
    }
}
