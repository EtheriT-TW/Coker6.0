using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.Application.Shared.Processor;
using HtmlAgilityPack;
using System.Net;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.Public.Helpers
{
    internal static class SeoMetaDescription
    {
        private const int MaximumLength = 200;
        private const int MaximumManualDescriptionLength = 320;
        private const int MaximumCandidateCount = 100;
        private const int MinimumCandidateLength = 20;

        public static async Task<string> BuildAsync(
            IHtmlProcessor htmlProcessor,
            string? description,
            string? contentHtml,
            string? siteDescription,
            string? pageTitle,
            string? locale,
            Func<IReadOnlyCollection<long>, Task<List<DirectoryGetDataDto>>> getDirectories)
        {
            var normalizedTitle = NormalizePlainText(pageTitle);
            var normalizedDescription = NormalizePlainText(description);

            // 有效的人工描述不需要解析頁面 HTML。
            if (!IsWeakDescription(normalizedDescription, normalizedTitle))
            {
                return Truncate(normalizedDescription);
            }

            var analysis = AnalyzeContent(
                htmlProcessor,
                contentHtml,
                normalizedTitle,
                locale);

            if (analysis.TextParts.Count > 0)
            {
                return JoinAndTruncate(analysis.TextParts);
            }

            if (analysis.DirectoryIds.Count > 0)
            {
                var directories = await getDirectories(analysis.DirectoryIds);
                var directoryDescription = BuildDirectoryDescription(
                    directories,
                    normalizedTitle,
                    locale);

                if (!string.IsNullOrWhiteSpace(directoryDescription))
                {
                    return Truncate(directoryDescription);
                }
            }

            var fallbackParts = new List<string>();
            AddDistinctPart(fallbackParts, normalizedTitle);
            AddDistinctPart(fallbackParts, NormalizePlainText(siteDescription));
            return JoinAndTruncate(fallbackParts);
        }

        private static ContentAnalysis AnalyzeContent(
            IHtmlProcessor htmlProcessor,
            string? contentHtml,
            string pageTitle,
            string? locale)
        {
            var output = new ContentAnalysis();
            if (string.IsNullOrWhiteSpace(contentHtml))
            {
                return output;
            }

            var document = htmlProcessor.LoadHtml(WebUtility.HtmlDecode(contentHtml));
            output.DirectoryIds.AddRange(
                (document.DocumentNode.SelectNodes("//*[@data-dirid]")
                    ?? Enumerable.Empty<HtmlNode>())
                .Select(node => node.GetAttributeValue("data-dirid", 0L))
                .Where(id => id > 0)
                .Distinct()
            );

            RemoveNonContentNodes(document);

            var candidates = (document.DocumentNode.SelectNodes(
                    "//p|//article|//main|//section|//blockquote|//div") ??
                Enumerable.Empty<HtmlNode>())
                .Take(MaximumCandidateCount)
                .Select((node, index) => CreateCandidate(node, index, pageTitle, locale))
                .Where(candidate => candidate != null)
                .Cast<TextCandidate>()
                .OrderByDescending(candidate => candidate.Score)
                .ThenBy(candidate => candidate.Index)
                .ToList();

            foreach (var candidate in candidates)
            {
                if (candidate.Score < 0)
                {
                    continue;
                }

                AddDistinctPart(output.TextParts, candidate.Text);
                if (string.Join("。", output.TextParts).Length >= MaximumLength)
                {
                    break;
                }
            }

            return output;
        }

        private static void RemoveNonContentNodes(HtmlDocument document)
        {
            var nodes = document.DocumentNode.SelectNodes(
                "//script|//style|//template|//noscript|//*[@hidden]|//*[@aria-hidden='true']|" +
                "//*[contains(concat(' ', normalize-space(@class), ' '), ' d-none ')]|" +
                "//*[contains(concat(' ', normalize-space(@class), ' '), ' material-symbols-outlined ')]|" +
                "//*[contains(concat(' ', normalize-space(@class), ' '), ' anchor_directory ')]|" +
                "//*[contains(concat(' ', normalize-space(@class), ' '), ' backstageType ')]|" +
                "//*[contains(concat(' ', normalize-space(@class), ' '), ' catalog_frame ')]|" +
                "//nav|//header|//footer|//button|//form");

            if (nodes == null)
            {
                return;
            }

            foreach (var node in nodes.ToList())
            {
                node.Remove();
            }
        }

        private static TextCandidate? CreateCandidate(
            HtmlNode node,
            int index,
            string pageTitle,
            string? locale)
        {
            var text = NormalizePlainText(node.InnerText);
            if (text.Length < MinimumCandidateLength ||
                NormalizeForComparison(text) == NormalizeForComparison(pageTitle))
            {
                return null;
            }

            var descendantCandidate = node.SelectNodes(
                    ".//p|.//article|.//section|.//blockquote|.//div")?
                .Any(child => NormalizePlainText(child.InnerText).Length >= MinimumCandidateLength) == true;

            // 容器內已有更精確的文字節點時，避免父容器把整頁標題一併串入。
            if (descendantCandidate && node.Name is "div" or "main" or "section")
            {
                return null;
            }

            var className = node.GetAttributeValue("class", string.Empty);
            var linkTextLength = node.SelectNodes(".//a")?
                .Sum(link => NormalizePlainText(link.InnerText).Length) ?? 0;
            var score = Math.Min(text.Length, MaximumLength);

            if (IsLocaleMatch(text, locale))
            {
                score += 80;
            }

            if (node.Name is "p" or "article" or "blockquote")
            {
                score += 35;
            }

            if (Regex.IsMatch(
                className,
                @"(^|[\s_-])(text|content|description|intro|summary|article)([\s_-]|$)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                score += 30;
            }

            if (Regex.IsMatch(
                className,
                @"(^|[\s_-])(title|menu|nav|breadcrumb|button|toolbar|pagination)([\s_-]|$)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                score -= 120;
            }

            if (linkTextLength > text.Length / 2)
            {
                score -= 100;
            }

            var digitCount = text.Count(char.IsDigit);
            if (digitCount > 0 && digitCount * 3 > text.Length)
            {
                score -= 80;
            }

            return new TextCandidate(index, text, score);
        }

        private static string BuildDirectoryDescription(
            IReadOnlyCollection<DirectoryGetDataDto>? directories,
            string pageTitle,
            string? locale)
        {
            var directory = directories?
                .Where(item => item.Visible)
                .OrderBy(item => item.Id)
                .FirstOrDefault();
            if (directory == null)
            {
                return string.Empty;
            }

            var title = NormalizePlainText(directory.Title);
            if (string.IsNullOrWhiteSpace(title))
            {
                title = pageTitle;
            }

            var directoryDescription = NormalizePlainText(directory.Description);
            if (!IsWeakDescription(directoryDescription, title))
            {
                return directoryDescription;
            }

            var isChinese = IsChineseLocale(locale);
            return (DirectoryTypeEnum)directory.Type switch
            {
                DirectoryTypeEnum.文章 => isChinese
                    ? $"本頁提供「{title}」相關文章目錄，包含最新消息、公告及相關內容。"
                    : $"Browse articles, news, announcements, and related content in {title}.",
                DirectoryTypeEnum.商品 => isChinese
                    ? $"本頁提供「{title}」相關商品目錄、產品介紹與選購資訊。"
                    : $"Browse products, product details, and shopping information in {title}.",
                DirectoryTypeEnum.選單 => isChinese
                    ? $"本頁提供「{title}」相關內容目錄與導覽資訊。"
                    : $"Browse content and navigation information for {title}.",
                _ => isChinese
                    ? $"本頁提供「{title}」相關內容與資訊。"
                    : $"Browse content and information related to {title}."
            };
        }

        private static bool IsWeakDescription(string description, string title)
        {
            if (string.IsNullOrWhiteSpace(description) ||
                description.Length > MaximumManualDescriptionLength)
            {
                return true;
            }

            if (NormalizeForComparison(description) == NormalizeForComparison(title))
            {
                return true;
            }

            if (ContainsCjkText(description))
            {
                return description.Count(char.IsLetterOrDigit) < 20;
            }

            return Regex.Matches(
                description,
                @"[\p{L}\p{N}]+(?:['’\-][\p{L}\p{N}]+)*"
            ).Count < 8;
        }

        private static bool IsLocaleMatch(string value, string? locale)
        {
            return IsChineseLocale(locale)
                ? ContainsCjkText(value)
                : !ContainsCjkText(value);
        }

        private static bool IsChineseLocale(string? locale)
        {
            return string.IsNullOrWhiteSpace(locale) ||
                locale.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsCjkText(string value)
        {
            return Regex.IsMatch(
                value ?? string.Empty,
                @"[\u3400-\u4DBF\u4E00-\u9FFF\u3040-\u30FF\uAC00-\uD7AF]");
        }

        private static string NormalizePlainText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var decoded = WebUtility.HtmlDecode(value);
            if (decoded.IndexOf('<') >= 0 && decoded.IndexOf('>') > decoded.IndexOf('<'))
            {
                decoded = Regex.Replace(decoded, @"<[^>]+>", " ");
            }

            return Regex.Replace(decoded, @"\s+", " ").Trim();
        }

        private static string NormalizeForComparison(string value)
        {
            return new string((value ?? string.Empty)
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());
        }

        private static void AddDistinctPart(List<string> parts, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var normalizedValue = NormalizeForComparison(value);
            if (parts.Any(part =>
                NormalizeForComparison(part) == normalizedValue ||
                NormalizeForComparison(part).Contains(normalizedValue) ||
                normalizedValue.Contains(NormalizeForComparison(part))))
            {
                return;
            }

            parts.Add(value.Trim().TrimEnd('。', '.', '，', ',', '；', ';'));
        }

        private static string JoinAndTruncate(IReadOnlyCollection<string> parts)
        {
            var separator = parts.Any(ContainsCjkText) ? "。" : ". ";
            return Truncate(string.Join(separator, parts));
        }

        private static string Truncate(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= MaximumLength)
            {
                return value;
            }

            var truncated = value.Substring(0, MaximumLength).TrimEnd();
            var lastBoundary = truncated.LastIndexOfAny(
                new[] { ' ', '。', '，', ',', '；', ';' });

            if (lastBoundary >= MaximumLength / 2)
            {
                truncated = truncated.Substring(0, lastBoundary).TrimEnd();
            }

            return truncated;
        }

        private sealed class ContentAnalysis
        {
            public List<string> TextParts { get; } = new();
            public List<long> DirectoryIds { get; } = new();
        }

        private sealed record TextCandidate(int Index, string Text, int Score);
    }
}
