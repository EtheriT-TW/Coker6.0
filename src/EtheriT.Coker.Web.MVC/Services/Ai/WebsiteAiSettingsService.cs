using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Common;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.MVC.Services.Ai;

public sealed class AiSettingsException(string message) : Exception(message) { }

// Never return or log this object; only the explicitly projected public view may leave the server.
public sealed class WebsiteAiSettings
{
    public bool Enabled { get; set; }
    public string Provider { get; set; } = "self";
    public string Model { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ClaudeWorkspaceId { get; set; } = "";
    public string Revision { get; set; } = "";
    public IReadOnlyList<AiProviderOption> Providers { get; set; } = [];
    public string EffectiveProvider => string.IsNullOrEmpty(ApiKey) ? "self" : Provider;
    public AiSettingsView Public(long websiteId) => new(websiteId, Enabled, Provider, Model, ApiKey.Length > 0, Revision, Providers, ClaudeWorkspaceId);
}

public sealed record AiProviderOption(string Key, string Name);
public sealed record AiSettingsView(long WebsiteId, bool Enabled, string Provider, string Model, bool HasApiKey, string Revision, IReadOnlyList<AiProviderOption> Providers, string ClaudeWorkspaceId);

public sealed class AiSettingsInput
{
    public string ClaudeWorkspaceId { get; set; } = "";
    public long WebsiteId { get; set; }
    public bool Enabled { get; set; }
    public string Provider { get; set; } = "self";
    public string Model { get; set; } = "";
    public string KeyAction { get; set; } = "keep";
    public string ApiKey { get; set; } = "";
    public string Revision { get; set; } = "";
}

public sealed class WebsiteAiSettingsService(CokerDbContext db, LoginUserData user, IDataProtectionProvider protection)
{
    private IDataProtector Protector(long websiteId, string provider) => protection.CreateProtector("Coker.WebsiteAi.ApiKey.v2",
        websiteId.ToString(System.Globalization.CultureInfo.InvariantCulture), provider);

    private async Task<List<StoreSet>> Definitions(CancellationToken token)
    {
        var definitions = await db.StoreSet.AsNoTracking().Include(s => s.storeSetItems)
            .Where(s => !s.IsDeleted && !s.storeSetGroup.IsDeleted && AiStoreSettingNames.All.Contains(s.key)).ToListAsync(token);
        if (definitions.Count != AiStoreSettingNames.All.Length || definitions.Select(s => s.key).Distinct().Count() != definitions.Count ||
            definitions.Select(s => s.FK_StoreSetGroupId).Distinct().Count() != 1)
            throw new AiSettingsException("AI 設定定義尚未完整部署，請管理員確認 AI 設定群組、參數與選項的 Seed 資料。");
        return definitions;
    }

    public async Task<WebsiteAiSettings> GetAsync(CancellationToken token)
    {
        var site = await user.GetWebsiteId();
        if (site <= 0) throw new AiSettingsException("請先選擇站台。");
        var definitions = await Definitions(token);
        var ids = definitions.Select(s => s.Id).ToArray();
        var details = await db.StoreSetDetail.AsNoTracking()
            .Where(d => !d.IsDeleted && d.FK_WebsiteId == site && ids.Contains(d.FK_StoreSetId)).ToListAsync(token);
        if (details.GroupBy(d => d.FK_StoreSetId).Any(g => g.Count() > 1))
            throw new AiSettingsException("本站 AI 設定有重複資料，請管理員確認。");
        string Value(string key)
        {
            var definition = definitions.Single(s => s.key == key);
            var detail = details.SingleOrDefault(d => d.FK_StoreSetId == definition.Id);
            return detail?.value ?? definition.storeSetItems.Where(i => !i.IsDeleted && i.IsDefault).Select(i => i.Key).SingleOrDefault()
                ?? definition.DefaultValue ?? "";
        }
        var options = definitions.Single(s => s.key == AiStoreSettingNames.Provider).storeSetItems
            .Where(i => !i.IsDeleted && i.Key is "self" or "openai" or "gemini" or "claude")
            .OrderBy(i => i.Id).Select(i => new AiProviderOption(i.Key, i.Value)).ToArray();
        if (options.Length != 4 || options.Select(o => o.Key).Distinct().Count() != 4)
            throw new AiSettingsException("AI 供應商選項尚未完整部署，請管理員確認 Seed 資料。");
        var enabledValue = Value(AiStoreSettingNames.Enabled);
        if (!bool.TryParse(enabledValue, out var enabled)) throw new AiSettingsException("AI 開放設定格式不正確。");
        var settings = new WebsiteAiSettings { Enabled = enabled && details.Any(d => d.FK_StoreSetId == definitions.Single(s => s.key == AiStoreSettingNames.Enabled).Id),
            Provider = Value(AiStoreSettingNames.Provider), Model = Value(AiStoreSettingNames.Model),
            ClaudeWorkspaceId = Value(AiStoreSettingNames.ClaudeWorkspaceId), Providers = options };
        // Revision is a fingerprint of the persisted rows, not another business parameter.
        settings.Revision = details.Count == 0 ? "" : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(
            details.OrderBy(d => d.FK_StoreSetId).Select(d => new { d.Id, d.FK_StoreSetId, d.value, d.enable, d.LastModificationTime })))));
        try
        {
            var encryptedKey = Value(AiStoreSettingNames.ApiKey);
            settings.ApiKey = encryptedKey.Length == 0 ? "" : Protector(site, settings.Provider).Unprotect(encryptedKey);
            Validate(settings);
            return settings;
        }
        catch (Exception ex) when (ex is CryptographicException or JsonException or ArgumentException)
        {
            throw new AiSettingsException("AI 設定無法解密或格式異常，請管理員確認加密金鑰與備份。");
        }
    }

    public static WebsiteAiSettings Prepare(WebsiteAiSettings current, AiSettingsInput input, bool requireModel = true)
    {
        if (input.Provider is not ("self" or "openai" or "gemini" or "claude") ||
            input.KeyAction is not ("keep" or "replace" or "clear") || input.Revision != current.Revision)
            throw new AiSettingsException("設定已變更或輸入不正確，請重新載入後再試。");
        if (input.Provider != current.Provider && current.ApiKey.Length > 0 && input.KeyAction == "keep")
            throw new AiSettingsException("切換供應商時，請更換金鑰或清除金鑰並使用系統預設 AI。");
        var key = input.KeyAction switch
        {
            "replace" => input.ApiKey?.Trim() ?? "",
            "clear" => "",
            _ => current.ApiKey
        };
        if (input.KeyAction == "replace" && key.Length == 0) throw new AiSettingsException("請輸入新的 API Key。");
        if (input.KeyAction != "replace" && !string.IsNullOrEmpty(input.ApiKey)) throw new AiSettingsException("請選擇更換金鑰後再輸入。");
        if (input.Provider == "self" && key.Length > 0) throw new AiSettingsException("選擇系統預設 AI 前，請清除外部金鑰。");
        var settings = new WebsiteAiSettings { Enabled = input.Enabled, Provider = input.Provider,
            Model = input.Model?.Trim() ?? "", ApiKey = key, Revision = current.Revision, Providers = current.Providers,
            ClaudeWorkspaceId = input.Provider == "claude" ? input.ClaudeWorkspaceId?.Trim() ?? "" : "" };
        Validate(settings, requireModel);
        ValidateApiKey(settings.ApiKey);
        return settings;
    }

    internal static void ValidateApiKey(string key)
    {
        if (key.Any(c => c < '!' || c > '~'))
            throw new AiSettingsException("API Key 含有不支援的字元，請從供應商複製完整金鑰，更換後儲存；不可包含中文、全形字元、空白或遮罩符號。");
    }

    private static void Validate(WebsiteAiSettings settings, bool requireModel = true)
    {
        ValidateClaudeWorkspaceId(settings.ClaudeWorkspaceId);
        if (settings.Provider is not ("self" or "openai" or "gemini" or "claude") || settings.ApiKey == null || settings.Model == null ||
            settings.ApiKey.Length > 4096 || settings.ApiKey.Any(char.IsWhiteSpace) || settings.ApiKey.Any(char.IsControl) ||
            settings.Model.Length > 150 || (settings.Model.Length > 0 && !Regex.IsMatch(settings.Model, "^[a-zA-Z0-9][a-zA-Z0-9._:-]*$")) ||
            (settings.ApiKey.Length > 0 && (settings.Provider == "self" || (requireModel && settings.Model.Length == 0))))
            throw new AiSettingsException("AI 供應商、金鑰或模型格式不正確；使用外部金鑰時必須填寫模型 ID。");
    }

    internal static void ValidateClaudeWorkspaceId(string workspaceId)
    {
        if (workspaceId == null || workspaceId.Length > 100 ||
            (workspaceId.Length > 0 && !Regex.IsMatch(workspaceId, "\\Awrkspc_[a-zA-Z0-9]+\\z")))
            throw new AiSettingsException("Claude 工作區 ID 格式不正確，請從 Claude Console 複製完整的工作區 ID。");
    }

    public async Task<AiSettingsView> SaveAsync(AiSettingsInput input, CancellationToken token)
    {
        var site = await user.GetWebsiteId();
        if (site <= 0 || site != input.WebsiteId) throw new AiSettingsException("站台已切換，請重新載入設定。");
        var userId = await user.GetUserId();
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            var definitions = await Definitions(token);
            var ids = definitions.Select(s => s.Id).ToArray();
            // Only this request's settings entities are tracked here. Retry from fresh database state.
            foreach (var entry in db.ChangeTracker.Entries<StoreSetDetail>().Where(e => e.Entity.FK_WebsiteId == site && ids.Contains(e.Entity.FK_StoreSetId)).ToList())
                entry.State = EntityState.Detached;
            await using var transaction = db.Database.IsRelational()
                ? await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, token) : null;
            var current = await GetAsync(token);
            var settings = Prepare(current, input);
            var details = await db.StoreSetDetail.Where(d => !d.IsDeleted && d.FK_WebsiteId == site && ids.Contains(d.FK_StoreSetId)).ToListAsync(token);
            var values = new Dictionary<string, string>
            {
                [AiStoreSettingNames.Enabled] = settings.Enabled ? "true" : "false",
                [AiStoreSettingNames.Provider] = settings.Provider,
                [AiStoreSettingNames.Model] = settings.Model,
                [AiStoreSettingNames.ClaudeWorkspaceId] = settings.ClaudeWorkspaceId,
                [AiStoreSettingNames.ApiKey] = settings.ApiKey.Length == 0 ? "" : Protector(site, settings.Provider).Protect(settings.ApiKey)
            };
            foreach (var definition in definitions)
            {
                var detail = details.SingleOrDefault(d => d.FK_StoreSetId == definition.Id);
                if (detail == null)
                {
                    detail = new StoreSetDetail { FK_WebsiteId = site, FK_StoreSetId = definition.Id, CreatorUserId = userId, CreationTime = DateTime.Now };
                    db.StoreSetDetail.Add(detail);
                }
                detail.value = values[definition.key];
                detail.enable = true;
                detail.LastModifierUserId = userId;
                detail.LastModificationTime = DateTime.Now;
            }
            await db.SaveChangesAsync(token);
            var saved = (await GetAsync(token)).Public(site);
            if (transaction != null) await transaction.CommitAsync(token);
            return saved;
        });
    }
}
