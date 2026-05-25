using EtheriT.Coker.Application.Processor.Option;
using EtheriT.Coker.Application.Shared.Processor;
using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Application.Processor
{
    public class HtmlSanitizer : IHtmlSanitizer
    {
        private readonly HtmlSanitizeOptions options = new();

        public string SanitizePublicHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            html = ExtractBodyInnerHtmlIfExists(html);

            var doc = new HtmlDocument
            {
                OptionFixNestedTags = true
            };

            doc.LoadHtml(html);

            RemoveBackstageNodes(doc);
            RemoveDisallowedNodes(doc);
            SanitizeAttributes(doc);
            NormalizeLinks(doc);
            NormalizeIframes(doc);

            return doc.DocumentNode.OuterHtml;
        }

        public string SanitizePublicCss(string css)
        {
            if (string.IsNullOrWhiteSpace(css))
                return string.Empty;

            var output = css;

            // 不允許從 CSS 再拉外部樣式，避免繞過 CSP / 樣式治理。
            output = Regex.Replace(output, @"@import\s+[^;]+;", "", RegexOptions.IgnoreCase);

            // 移除 CSS 中明顯危險語法。
            output = Regex.Replace(output, @"javascript\s*:", "", RegexOptions.IgnoreCase);
            output = Regex.Replace(output, @"vbscript\s*:", "", RegexOptions.IgnoreCase);
            output = Regex.Replace(output, @"expression\s*\(", "", RegexOptions.IgnoreCase);
            output = Regex.Replace(output, @"behavior\s*:", "", RegexOptions.IgnoreCase);
            output = Regex.Replace(output, @"-moz-binding\s*:", "", RegexOptions.IgnoreCase);

            // 避免 CSS url(data:text/html...) 這類內容。
            output = Regex.Replace(output, @"url\s*\(\s*['""]?\s*data\s*:\s*text/html[^)]*\)", "", RegexOptions.IgnoreCase);
            output = Regex.Replace(output, @"url\s*\(\s*['""]?\s*data\s*:\s*application/xhtml\+xml[^)]*\)", "", RegexOptions.IgnoreCase);

            return output;
        }

        private string ExtractBodyInnerHtmlIfExists(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var doc = new HtmlDocument
            {
                OptionFixNestedTags = true
            };

            doc.LoadHtml(html);

            var body = doc.DocumentNode.SelectSingleNode("//body");

            if (body == null)
                return html;

            return body.InnerHtml ?? string.Empty;
        }

        private void RemoveBackstageNodes(HtmlDocument doc)
        {
            if (!options.RemoveBackstageNodes)
                return;

            var nodes = doc.DocumentNode
                .Descendants()
                .Where(node => HasClass(node, "backstageType"))
                .ToList();

            foreach (var node in nodes)
            {
                node.Remove();
            }
        }

        private void RemoveDisallowedNodes(HtmlDocument doc)
        {
            var nodes = doc.DocumentNode
                .Descendants()
                .Where(node =>
                    node.NodeType == HtmlNodeType.Element &&
                    !options.AllowedTags.Contains(node.Name)
                )
                .ToList();

            foreach (var node in nodes)
            {
                node.Remove();
            }
        }

        private void SanitizeAttributes(HtmlDocument doc)
        {
            var nodes = doc.DocumentNode
                .DescendantsAndSelf()
                .Where(node => node.NodeType == HtmlNodeType.Element && node.HasAttributes)
                .ToList();

            foreach (var node in nodes)
            {
                var removeNames = node.Attributes
                    .Where(attr => ShouldRemoveAttribute(node, attr))
                    .Select(attr => attr.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var attrName in removeNames)
                {
                    node.Attributes.Remove(attrName);
                }
            }
        }

        private bool ShouldRemoveAttribute(HtmlNode node, HtmlAttribute attr)
        {
            var tagName = NormalizeName(node.Name);
            var attrName = NormalizeName(attr.Name);
            var value = attr.Value?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(attrName))
                return true;

            // 最高優先：明確禁止。這必須早於 data-* / aria-* prefix 判斷，
            // 否則 data-bs-html 會被 data-* 放行。
            if (options.BlockedAttributes.Contains(attrName))
                return true;

            // inline event 一律禁止：onclick / onload / onerror / onchange...
            // 這不是 option，因為系統規則就是 GrapesJS 不接受 JS。
            if (attrName.StartsWith("on", StringComparison.OrdinalIgnoreCase))
                return true;

            if (!IsAllowedAttribute(tagName, attrName))
                return true;

            // 原生 URL 屬性檢查。
            if (options.UrlAttributes.Contains(attrName))
            {
                if (attrName.Equals("srcset", StringComparison.OrdinalIgnoreCase))
                    return !IsSafeSrcSet(value);

                return IsDangerousUrl(value);
            }

            // 系統 data-* 可能被前端 JS 轉成 href/src，所以額外檢查。
            if (options.DataUrlAttributes.Contains(attrName))
            {
                return IsDangerousUrl(value);
            }

            return false;
        }

        private bool IsAllowedAttribute(string tagName, string attrName)
        {
            if (options.GlobalAttributes.Contains(attrName))
                return true;

            if (IsAllowedAttributePrefix(attrName))
                return true;

            if (options.TagAttributes.TryGetValue(tagName, out var tagAttrs))
                return tagAttrs.Contains(attrName);

            return false;
        }

        private bool IsAllowedAttributePrefix(string attrName)
        {
            return options.GlobalAttributePrefixes.Any(prefix =>
                attrName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            );
        }

        private bool IsDangerousUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalized = NormalizeUrlValue(value);

            if (string.IsNullOrWhiteSpace(normalized))
                return false;

            var lower = normalized.ToLowerInvariant();

            if (lower.StartsWith("javascript:"))
                return true;

            if (lower.StartsWith("vbscript:"))
                return true;

            if (lower.StartsWith("data:"))
                return !IsAllowedDataImage(lower);

            // #anchor
            if (lower.StartsWith("#"))
                return false;

            // /path 或 //cdn.example.com/path
            // // 會走目前頁面 protocol，domain 是否允許交由 CSP 控制。
            if (lower.StartsWith("/"))
                return false;

            // ./path 或 ../path
            if (lower.StartsWith("./") || lower.StartsWith("../"))
                return false;

            // 絕對 URL：檢查 scheme。
            if (Uri.TryCreate(normalized, UriKind.Absolute, out var absoluteUri))
            {
                return !options.AllowedUrlSchemes.Contains(absoluteUri.Scheme);
            }

            // 相對路徑，例如 upload/xxx.png、images/a.jpg、page/key。
            // 但如果冒號出現在 / ? # 之前，視為未知 scheme，移除。
            var colonIndex = normalized.IndexOf(':');
            if (colonIndex >= 0)
            {
                var slashIndex = IndexOfAnyOrMax(normalized, '/', '?', '#');
                if (colonIndex < slashIndex)
                    return true;
            }

            return false;
        }

        private bool IsSafeSrcSet(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            var urls = ExtractSrcSetUrls(value);

            if (!urls.Any())
                return false;

            return urls.All(url => !IsDangerousUrl(url));
        }

        private List<string> ExtractSrcSetUrls(string srcset)
        {
            var urls = new List<string>();

            if (string.IsNullOrWhiteSpace(srcset))
                return urls;

            var i = 0;
            var len = srcset.Length;

            while (i < len)
            {
                // skip spaces / commas
                while (i < len && (char.IsWhiteSpace(srcset[i]) || srcset[i] == ','))
                    i++;

                if (i >= len)
                    break;

                var start = i;

                // URL part ends at whitespace.
                // data:image/svg+xml,<svg...> 內部可能有 comma，所以不能用 comma 切 URL。
                while (i < len && !char.IsWhiteSpace(srcset[i]))
                    i++;

                var url = srcset.Substring(start, i - start).Trim();

                if (!string.IsNullOrWhiteSpace(url))
                    urls.Add(url);

                // skip descriptor until comma
                while (i < len && srcset[i] != ',')
                    i++;

                if (i < len && srcset[i] == ',')
                    i++;
            }

            return urls;
        }

        private bool IsAllowedDataImage(string lowerUrl)
        {
            return options.AllowedDataImagePrefixes.Any(prefix =>
                lowerUrl.StartsWith(prefix.ToLowerInvariant())
            );
        }

        private string NormalizeUrlValue(string value)
        {
            var decoded = HtmlEntity.DeEntitize(value ?? "");

            // 移除控制字元與空白，避免 java script: 這類繞過。
            decoded = Regex.Replace(decoded, @"[\u0000-\u001F\u007F\s]+", "");

            return decoded.Trim();
        }

        private int IndexOfAnyOrMax(string value, params char[] chars)
        {
            var indexes = chars
                .Select(c => value.IndexOf(c))
                .Where(index => index >= 0)
                .ToList();

            return indexes.Count == 0 ? int.MaxValue : indexes.Min();
        }

        private void NormalizeLinks(HtmlDocument doc)
        {
            if (!options.ForceNoopenerForBlankTarget)
                return;

            var links = doc.DocumentNode
                .Descendants("a")
                .Where(link =>
                    string.Equals(
                        link.GetAttributeValue("target", ""),
                        "_blank",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .ToList();

            foreach (var link in links)
            {
                var rel = link.GetAttributeValue("rel", "");
                var tokens = SplitTokens(rel);

                tokens.Add("noopener");
                tokens.Add("noreferrer");

                link.SetAttributeValue("rel", string.Join(" ", tokens));
            }
        }

        private void NormalizeIframes(HtmlDocument doc)
        {
            var iframes = doc.DocumentNode
                .Descendants("iframe")
                .ToList();

            foreach (var iframe in iframes)
            {
                if (string.IsNullOrWhiteSpace(iframe.GetAttributeValue("title", "")))
                {
                    iframe.SetAttributeValue("title", "Embedded content");
                }

                if (string.IsNullOrWhiteSpace(iframe.GetAttributeValue("loading", "")))
                {
                    iframe.SetAttributeValue("loading", "lazy");
                }

                if (string.IsNullOrWhiteSpace(iframe.GetAttributeValue("referrerpolicy", "")))
                {
                    iframe.SetAttributeValue("referrerpolicy", "strict-origin-when-cross-origin");
                }
            }
        }

        private HashSet<string> SplitTokens(string value)
        {
            return new HashSet<string>(
                (value ?? "")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Trim())
                    .Where(v => !string.IsNullOrWhiteSpace(v)),
                StringComparer.OrdinalIgnoreCase
            );
        }

        private bool HasClass(HtmlNode node, string className)
        {
            var classValue = node.GetAttributeValue("class", "");

            if (string.IsNullOrWhiteSpace(classValue))
                return false;

            return classValue
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Any(c => string.Equals(c, className, StringComparison.OrdinalIgnoreCase));
        }

        private string NormalizeName(string name)
        {
            return (name ?? "").Trim().ToLowerInvariant();
        }
    }
}