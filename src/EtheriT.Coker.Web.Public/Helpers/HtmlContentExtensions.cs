using Microsoft.AspNetCore.Html;
using System.Text.Encodings.Web;

namespace EtheriT.Coker.Web.Public.Helpers
{
    public static class HtmlContentExtensions
    {
        /// <summary>Razor partial 的渲染結果轉成字串，供 TagHelper 參數使用。</summary>
        public static string ToHtmlString(this IHtmlContent content)
        {
            using var writer = new StringWriter();
            content.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }
    }
}