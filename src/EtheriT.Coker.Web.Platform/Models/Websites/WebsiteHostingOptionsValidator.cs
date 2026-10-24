using EtheriT.Coker.Application.Shared.Dto.enumType;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Web.Platform.Models.Websites;

/// <summary>啟動時檢查 WebsiteHosting 與 Provisioning:Servers 組出的主機位置；設定錯就讓網站起不來，不要等到存檔才出錯。</summary>
public sealed class WebsiteHostingOptionsValidator(IConfiguration configuration) : IValidateOptions<WebsiteHostingOptions>
{
    private const string Section = WebsiteHostingOptions.SectionName;

    public ValidateOptionsResult Validate(string? name, WebsiteHostingOptions options)
    {
        var errors = new List<string>();
        ValidateHostLocations(options.AllHostLocations, errors);
        ValidateDefaultHostByLevel(options, errors);
        ValidateDefaultUrlSuffix(options.DefaultUrlSuffix, errors);
        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }

    private void ValidateHostLocations(IReadOnlyList<WebsiteHostLocationOption> hosts, List<string> errors)
    {
        // 舊設定留著會被默默忽略，直接擋下，避免以為改了有效
        if (configuration.GetSection($"{Section}:HostLocations").Exists())
            errors.Add($"{Section}:HostLocations 已停用：有 Worker 的主機改填 Provisioning:Servers 的 HostLocation，沒有 Worker 的放 {Section}:ArchiveLocations。");

        if (hosts.Count == 0)
        {
            errors.Add($"Provisioning:Servers 的 HostLocation 與 {Section}:ArchiveLocations 合計至少要有一筆主機位置。");
            return;
        }

        foreach (var host in hosts)
        {
            var source = host.HasWorker
                ? $"Provisioning:Servers（{host.ServerId}）的 HostLocation"
                : $"{Section}:ArchiveLocations";
            if (string.IsNullOrWhiteSpace(host.Value) || host.Value != host.Value.Trim())
                errors.Add($"{source}「{host.Value}」不可空白，前後也不能有空白。");
        }

        var duplicates = hosts
            .GroupBy(x => x.Value, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
        foreach (var value in duplicates)
            errors.Add($"主機位置「{value}」重複（Provisioning:Servers 的 HostLocation 與 {Section}:ArchiveLocations 不可重複）。");
    }

    private static void ValidateDefaultHostByLevel(WebsiteHostingOptions options, List<string> errors)
    {
        var hostValues = options.AllHostLocations.Select(x => x.Value).ToHashSet(StringComparer.Ordinal);

        foreach (var (key, host) in options.DefaultHostByLevel)
        {
            var isLevelName = !int.TryParse(key, out _)
                && Enum.TryParse<WebsiteLevelEnum>(key, out var level)
                && Enum.IsDefined(level);
            if (!isLevelName)
                errors.Add($"{Section}:DefaultHostByLevel 的 key「{key}」不是版本名稱（簡約／形象／會員／購物）。");
            else if (!hostValues.Contains(host))
                errors.Add($"{Section}:DefaultHostByLevel:{key} 的「{host}」不是可選主機位置（Servers 的 HostLocation 或 ArchiveLocations）。");
        }

        foreach (var level in Enum.GetValues<WebsiteLevelEnum>())
        {
            if (!options.DefaultHostByLevel.ContainsKey(level.ToString()))
                errors.Add($"{Section}:DefaultHostByLevel 缺少「{level}」。");
        }
    }

    private static void ValidateDefaultUrlSuffix(string? suffix, List<string> errors)
    {
        var isValid = !string.IsNullOrWhiteSpace(suffix)
            && suffix.Length > 1
            && suffix.StartsWith('.')
            && !suffix.EndsWith('.')
            && !suffix.Contains("..")
            && !suffix.Any(char.IsWhiteSpace);
        if (!isValid)
            errors.Add($"{Section}:DefaultUrlSuffix 必須以 . 開頭，例如 .coker.ezsale.tw。");
    }
}