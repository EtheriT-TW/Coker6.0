using EtheriT.Coker.Application.Shared.Processor;
using HtmlAgilityPack;

namespace EtheriT.Coker.Web.Public.Helpers
{
    internal static class ContentImageLoadingHtml
    {
        private static readonly string PicturePlaceholder = "data:image/svg+xml," + Uri.EscapeDataString(
            "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 160 90\"><rect width=\"160\" height=\"90\" fill=\"#f1f3f5\"/><path d=\"M47 65l24-27 17 18 11-12 24 21H47z\" fill=\"#adb5bd\"/><circle cx=\"108\" cy=\"29\" r=\"8\" fill=\"#adb5bd\"/></svg>");

        public static string Prepare(IHtmlProcessor htmlProcessor, string html)
        {
            var document = htmlProcessor.LoadHtml(html);
            var imageIndex = 0;
            foreach (var image in htmlProcessor.Find(document, "img"))
            {
                if (IsPictureGalleryImage(image))
                {
                    var src = image.GetAttributeValue("data-src", "");
                    if (string.IsNullOrWhiteSpace(src))
                    {
                        src = image.GetAttributeValue("src", "");
                    }
                    if (!string.IsNullOrWhiteSpace(src))
                    {
                        image.SetAttributeValue("data-src", src);
                        image.SetAttributeValue("src", PicturePlaceholder);
                        image.SetAttributeValue("data-coker-lazy", "unload");
                    }
                    image.Attributes.Remove("loading");
                    imageIndex++;
                    continue;
                }

                // 保留前四張作為首屏保護；實際主視覺可用 loading="eager" 明確排除。
                var isLeadingImage = imageIndex++ < 4;
                if (isLeadingImage || image.Attributes["loading"] != null ||
                    image.Attributes["data-src"] != null ||
                    string.IsNullOrWhiteSpace(image.GetAttributeValue("src", "")) ||
                    image.GetAttributeValue("fetchpriority", "").Equals("high", StringComparison.OrdinalIgnoreCase) ||
                    HasIndependentLoading(image))
                {
                    continue;
                }

                // 尺寸交由既有 RWD CSS 控制，保留 src 供輪播與燈箱读取。
                image.SetAttributeValue("loading", "lazy");
            }

            return document.DocumentNode.OuterHtml;
        }

        private static bool IsPictureGalleryImage(HtmlNode image)
        {
            return image.Ancestors("a").Any(anchor => anchor
                .GetAttributeValue("class", "")
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Contains("imageItem", StringComparer.Ordinal));
        }

        private static bool HasIndependentLoading(HtmlNode image)
        {
            foreach (var node in image.AncestorsAndSelf())
            {
                if (node.Attributes["data-no-lazy"] != null)
                {
                    return true;
                }

                var classes = node.GetAttributeValue("class", "")
                    .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                // 固定等高輪播會在初始化時量測高度，先保留其既有載入方式。
                if (classes.Any(c => c is "modal" or "article-viewer" or "js-gallery3d-page" or
                    "gallery3d" or "gallery3d-stage" or "gallery3d-wrapper" or "swiper_same_height"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
