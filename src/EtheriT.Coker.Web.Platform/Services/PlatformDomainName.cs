using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.Platform.Services;

/// <summary>
/// 網址 → 主機名稱 → 候選網域。網站存檔、網域存檔、比對 API 共用，規則才不會分岔。
/// </summary>
public static partial class PlatformDomainName
{
    [GeneratedRegex(@"^(?=.{1,253}$)([a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z0-9-]{2,63}$")]
    private static partial Regex HostPattern();

    /// <summary>接受 example.com、https://www.example.com/path 等寫法；取不出合法主機名稱回 null（IP、localhost 也回 null）。</summary>
    public static string? ToHost(string? input)
    {
        var text = input?.Trim();
        if (string.IsNullOrEmpty(text))
            return null;

        if (!text.Contains("://", StringComparison.Ordinal))
            text = "http://" + text;

        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri) || uri.HostNameType != UriHostNameType.Dns)
            return null;

        // IdnHost：中文網域轉成 xn-- 形式，同一個網域才不會有兩種寫法
        var host = uri.IdnHost.TrimEnd('.').ToLowerInvariant();
        return HostPattern().IsMatch(host) ? host : null;
    }

    /// <summary>shop.example.com.tw → [shop.example.com.tw, example.com.tw, com.tw]：由長到短、至少兩段。</summary>
    public static IReadOnlyList<string> Candidates(string host)
    {
        var labels = host.Split('.');
        return Enumerable.Range(0, labels.Length - 1)
            .Select(skip => string.Join('.', labels.Skip(skip)))
            .ToList();
    }

    /// <summary>彈窗預填用。無法可靠判斷 .com.tw 這類後綴，只去掉 www.，由使用者自行修正。</summary>
    public static string Suggest(string host) =>
        host.StartsWith("www.", StringComparison.Ordinal) ? host[4..] : host;

    /// <summary>候選一次查完，取名稱最長（最精確）的那筆。</summary>
    public static async Task<PlatformDomain?> FindBestMatchAsync(
        this IQueryable<PlatformDomain> domains,
        string host,
        CancellationToken cancellationToken)
    {
        var candidates = Candidates(host);
        var matches = await domains
            .Where(domain => candidates.Contains(domain.DomainName))
            .ToListAsync(cancellationToken);

        return matches.MaxBy(domain => domain.DomainName.Length);
    }
}