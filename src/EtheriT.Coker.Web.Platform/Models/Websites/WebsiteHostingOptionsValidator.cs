using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Web.Platform.Models.Provisioning;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Web.Platform.Models.Websites;

/// <summary>啟動時檢查 WebsiteHosting；設定錯就讓網站起不來，不要等到存檔才出錯。</summary>
public sealed class WebsiteHostingOptionsValidator(IConfiguration configuration) : IValidateOptions<WebsiteHostingOptions>
{
    private const string Section = WebsiteHostingOptions.SectionName;

    public ValidateOptionsResult Validate(string? name, WebsiteHostingOptions options)
    {
        var errors = new List<string>();
        ValidateHostLocations(options.HostLocations, errors);
        ValidateDefaultHostByLevel(options, errors);
        ValidateDefaultUrlSuffix(options.DefaultUrlSuffix, errors);
        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }

    private void ValidateHostLocations(List<WebsiteHostLocationOption> hosts, List<string> errors)
    {
        if (hosts.Count == 0)
        {
            errors.Add($"{Section}:HostLocations 至少要有一筆。");
            return;
        }

        var serverIds = (configuration.GetSection("Provisioning:Servers").Get<List<ProvisioningServerOptions>>() ?? [])
            .Select(x => x.Id)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < hosts.Count; i++)
        {
            var host = hosts[i];
            var path = $"{Section}:HostLocations:{i}";
            if (string.IsNullOrWhiteSpace(host.Value) || host.Value != host.Value.Trim())
                errors.Add($"{path}:Value 不可空白，前後也不能有空白。");
            if (string.IsNullOrWhiteSpace(host.Text))
                errors.Add($"{path}:Text 不可空白。");
            if (host.HasWorker && !serverIds.Contains(host.ServerId!))
                errors.Add($"{path}:ServerId「{host.ServerId}」不在 Provisioning:Servers 裡；沒有 Worker 請填 null。");
        }

        var duplicates = hosts
            .GroupBy(x => x.Value, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
        foreach (var value in duplicates)
            errors.Add($"{Section}:HostLocations 的 Value「{value}」重複。");
    }

    private static void ValidateDefaultHostByLevel(WebsiteHostingOptions options, List<string> errors)
    {
        var hostValues = options.HostLocations.Select(x => x.Value).ToHashSet(StringComparer.Ordinal);

        foreach (var (key, host) in options.DefaultHostByLevel)
        {
            var isLevelName = !int.TryParse(key, out _)
                && Enum.TryParse<WebsiteLevelEnum>(key, out var level)
                && Enum.IsDefined(level);
            if (!isLevelName)
                errors.Add($"{Section}:DefaultHostByLevel 的 key「{key}」不是版本名稱（簡約／形象／會員／購物）。");
            else if (!hostValues.Contains(host))
                errors.Add($"{Section}:DefaultHostByLevel:{key} 的「{host}」不在 HostLocations 的 Value 裡。");
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