using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Common
{
    public static class HtmlNodeExtensions
    {
        public static string Attr(this HtmlNode? node, string name)
        {
            return node?.GetAttributeValue(name, "") ?? "";
        }
        public static string Attr(this HtmlNode? node, string name, string value)
        {
            if (node == null || string.IsNullOrEmpty(name))
                return "";

            node.SetAttributeValue(name, value);
            return value;
        }
    }
}
