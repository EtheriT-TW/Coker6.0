using System.Security.Cryptography;
using System.Text.Json;
using HtmlAgilityPack;

namespace EtheriT.Coker.Application.SiteCrawl;

public static class SiteCrawlFingerprint
{
    // 清理規則變更時調整版本，讓已有對照的來源內容重新比對一次。
    private const int RuleVersion = 1;
    private const int ListRuleVersion = 1;

    public static string ComputeSource(HtmlNode? content, string? title, DateTime? listDate = null,
        string? description = null, string? imageUrl = null)
    {
        var prepared = content == null ? null : SiteContentImporter.PrepareContent(content);
        RemoveVolatileMarkup(prepared);
        return Digest(new { RuleVersion, Html = Compact(prepared?.OuterHtml), Title = title?.Trim(),
            ListDate = listDate, Description = description?.Trim(), ImageUrl = imageUrl?.Trim() });
    }

    // 目錄項目由文章、商品或子選單分別更新。列表頁只搬目錄前的介紹，避免載入順序或分頁狀態影響指紋。
    public static string ComputeListSource(HtmlNode? content, string? title, string directoryKind, out HtmlNode? intro)
    {
        intro = content == null ? null : SiteImportTemplates.ExtractListIntro(content);
        var prepared = intro?.CloneNode(true);
        RemoveVolatileMarkup(prepared);
        return Digest(new { ListRuleVersion, DirectoryKind = directoryKind, Html = Compact(prepared?.InnerHtml),
            Title = title?.Trim() });
    }

    private static void RemoveVolatileMarkup(HtmlNode? node)
    {
        if (node == null) return;
        foreach (var child in node.Descendants().ToArray())
        {
            // JS 產生的搜尋 token 每次載入都不同，也不會成為新站表單的內容。
            if (child.NodeType == HtmlNodeType.Comment || child.Name == "input"
                && child.GetAttributeValue("type", "").Equals("hidden", StringComparison.OrdinalIgnoreCase)
                && child.GetAttributeValue("name", "").Equals("token", StringComparison.OrdinalIgnoreCase))
                child.Remove();
        }
    }

    public static string ComputeWritten(string? html, string? css) => Digest(new { Html = html, Css = css });

    private static string? Compact(string? value) =>
        value == null ? null : string.Concat(value.Where(character => !char.IsWhiteSpace(character)));

    private static string Digest<T>(T value) =>
        Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(value))).ToLowerInvariant();
}

internal sealed record SiteCrawlWritten(string? Hash, string? Title, SiteCrawlFields? Fields = null, string? DraftHash = null);
