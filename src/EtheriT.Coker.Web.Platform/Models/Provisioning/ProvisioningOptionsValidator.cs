using EtheriT.Coker.Application.Shared.Dto.enumType;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Web.Platform.Models.Provisioning;

public sealed class ProvisioningOptionsValidator(IConfiguration configuration) : IValidateOptions<ProvisioningOptions>
{
    public ValidateOptionsResult Validate(string? name, ProvisioningOptions options)
    {
        var errors = new List<string>();
        if (configuration.GetSection("WebsiteHosting").Exists())
            errors.Add("WebsiteHosting 已停用，請將主機位置與支援版本移至 Provisioning:Servers，網址尾碼移至 Provisioning:DefaultUrlSuffix。");
        if (configuration.GetSection("Provisioning:AgentApiKeys").Exists())
            errors.Add("Provisioning:AgentApiKeys 已停用，請將 ApiKey 移至各筆 Provisioning:Servers。");
        foreach (var server in options.Servers)
        {
            if (string.IsNullOrWhiteSpace(server.Id) || server.Id != server.Id.Trim())
                errors.Add("Provisioning:Servers 的 Id 不可空白或包含前後空白。");
            if (server.HostLocation is not null && (string.IsNullOrWhiteSpace(server.HostLocation)
                || server.HostLocation != server.HostLocation.Trim()))
                errors.Add($"伺服器 {server.Id} 的 HostLocation 不可空白或包含前後空白。");
            if (server.SupportedLevels.Length > 0 && string.IsNullOrWhiteSpace(server.HostLocation))
                errors.Add($"伺服器 {server.Id} 設定 SupportedLevels 時必須填寫 HostLocation。");
            foreach (var level in server.SupportedLevels)
                if (int.TryParse(level, out _) || !Enum.TryParse<WebsiteLevelEnum>(level, out var parsed) || !Enum.IsDefined(parsed))
                    errors.Add($"伺服器 {server.Id} 的 SupportedLevels「{level}」不是有效版本名稱。");
        }
        foreach (var group in options.Servers.GroupBy(s => s.Id, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
            errors.Add($"Provisioning:Servers 的 Id「{group.Key}」重複。");
        foreach (var group in options.AllHostLocations.GroupBy(h => h.Value, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
            errors.Add($"Provisioning:Servers 的 HostLocation「{group.Key}」重複。");
        if (options.AllHostLocations.Count == 0)
            errors.Add("Provisioning:Servers 至少需要一筆 HostLocation。");
        var suffix = options.DefaultUrlSuffix;
        if (string.IsNullOrWhiteSpace(suffix) || suffix.Length <= 1 || !suffix.StartsWith('.')
            || suffix.EndsWith('.') || suffix.Contains("..") || suffix.Any(char.IsWhiteSpace))
            errors.Add("Provisioning:DefaultUrlSuffix 必須以 . 開頭且不可包含空白，例如 .localhost。");
        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
