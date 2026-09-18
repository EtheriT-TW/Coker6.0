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

    /// <summary>網域存檔用：轉成主機名稱後再去掉 www.（網域本身不該帶 www）。</summary>
    public static string? ToDomainName(string? input)
    {
        var host = ToHost(input);
        if (host is null)
            return null;

        // 去掉後必須還有兩段以上，否則 www.tw 會變成無效的 tw
        var stripped = Suggest(host);
        return stripped.Contains('.') ? stripped : host;
    }

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

    /// <summary>
    /// 常見的「兩段式公共後綴」。列在這裡的，註冊網域要保留三段（example.com.tw），
    /// 其餘一律保留兩段（example.com）。清單外的冷門後綴會判斷錯，需要時再補。
    /// 與前端 ClientApp/src/utils/domain-name.ts 同一份清單，改了要兩邊一起改。
    /// </summary>
    private static readonly HashSet<string> TwoLabelSuffixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "com.tw", "net.tw", "org.tw", "gov.tw", "edu.tw", "idv.tw", "game.tw", "ebiz.tw", "club.tw",
        "com.cn", "net.cn", "org.cn",
        "com.hk", "com.sg", "com.my", "com.au", "com.br",
        "co.jp", "ne.jp", "or.jp",
        "co.kr", "co.uk", "org.uk", "co.nz", "co.th", "co.id", "co.in"
    };

    /// <summary>shop.example.com.tw → example.com.tw；blog.example.com → example.com。比對用的 Candidates() 不走這裡。</summary>
    public static string StripSubdomain(string host)
    {
        var labels = host.Split('.');
        if (labels.Length <= 2)
            return host;

        var keep = TwoLabelSuffixes.Contains(string.Join('.', labels[^2..])) ? 3 : 2;
        return labels.Length <= keep ? host : string.Join('.', labels[^keep..]);
    }

    /// <summary>彈窗預填用：去掉 www. 與子網域，取出註冊網域。</summary>
    public static string Suggest(string host) =>
        StripSubdomain(host.StartsWith("www.", StringComparison.Ordinal) ? host[4..] : host);


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