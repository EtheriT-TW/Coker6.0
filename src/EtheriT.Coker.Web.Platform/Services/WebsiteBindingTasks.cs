using System.Text.Json;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.Provisioning;
using EtheriT.Coker.Web.Platform.Models.Websites;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Platform.Services;

public sealed class WebsiteBindingTasks(CokerDbContext db, IConfiguration configuration)
{
    public async Task<WebsiteBindingComparison> ImportExistingUrlsAsync(Website site, CancellationToken cancellationToken, string? target = null)
    {
        var snapshot = await LoadSnapshotAsync(site, cancellationToken, target);
        // 等已送出的網址變更完成並取得新快照，避免把剛刪除的網址補回來。
        var unsettled = await db.ProvisioningTasks.AsNoTracking().Where(x => x.TargetServerId == snapshot.ServerId
            && x.Type == ProvisioningTaskType.SyncWebsiteBindings
            && (x.Status == ProvisioningTaskStatus.Pending || x.Status == ProvisioningTaskStatus.Running
                || x.CompletedAtUtc >= snapshot.LastSeenAtUtc.AddSeconds(-30)))
            .Select(x => x.PayloadJson).ToListAsync(cancellationToken);
        if (unsettled.Any(json => string.Equals(JsonSerializer.Deserialize<ProvisioningTaskPayload>(json)?.SiteName,
            snapshot.SiteName, StringComparison.OrdinalIgnoreCase)))
            return new WebsiteBindingComparison("Pending", "網址正在更新，請稍後重新開啟編輯頁查看最新結果。", null, null, [], []);
        var existing = await db.WebsiteAlternateUrls.Where(x => !x.IsDeleted && x.FK_WebsiteId == site.Id)
            .OrderBy(x => x.Sort).ToListAsync(cancellationToken);
        var known = existing.Select(x => PlatformDomainName.ToHost(x.Url)).OfType<string>()
            .Append(snapshot.Primary).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var hosts = snapshot.Bindings.Where(x => string.Equals(x.SiteName, snapshot.SiteName, StringComparison.OrdinalIgnoreCase))
            .SelectMany(x => x.HostNames).Select(PlatformDomainName.ToHost).OfType<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase).Where(host => !known.Contains(host)).OrderBy(x => x).ToArray();
        if (hosts.Any(host => $"https://{host}".Length > 255))
            throw new InvalidOperationException("主機上有超出長度限制的網址，無法自動更新網址清單，請聯絡管理員。");
        var sort = existing.Select(x => x.Sort).DefaultIfEmpty(-1).Max() + 1;
        foreach (var host in hosts)
        {
            if (snapshot.Bindings.Any(x => !string.Equals(x.SiteName, snapshot.SiteName, StringComparison.OrdinalIgnoreCase)
                && x.HostNames.Contains(host, StringComparer.OrdinalIgnoreCase)))
                throw new InvalidOperationException("網址同時設定在不同網站，請聯絡管理員確認後再更新清單。");
            var domain = await db.PlatformDomains.AsNoTracking().FindBestMatchAsync(host, cancellationToken);
            db.WebsiteAlternateUrls.Add(new WebsiteAlternateUrl
            {
                FK_WebsiteId = site.Id, Url = $"https://{host}", Sort = sort++, FK_PlatformDomainId = domain?.Id
            });
        }
        return ToComparison(snapshot);
    }

    public async Task<WebsiteBindingComparison> CompareAsync(Website site, CancellationToken cancellationToken, string? target = null)
    {
        try
        {
            var snapshot = await LoadSnapshotAsync(site, cancellationToken, target);
            return ToComparison(snapshot);
        }
        catch (SelectionRequiredException ex)
        {
            return new WebsiteBindingComparison("SelectionRequired", ex.Message, null, null, [], [], ex.Candidates);
        }
        catch (Exception ex) when (ex is InvalidOperationException or JsonException)
        {
            return new WebsiteBindingComparison("Unavailable", ex is JsonException ? "Worker 繫結資料無法辨識。" : ex.Message,
                null, null, [], []);
        }
    }

    public async Task<ProvisioningTask> CreateAsync(Website site, string? newLocation,
        IReadOnlyList<string> previous, IReadOnlyList<string> desired, CancellationToken cancellationToken, string? target = null)
    {
        if (site.HostLocation != newLocation?.Trim())
            throw new InvalidOperationException("網址與主機位置不能同時變更；請先完成網站搬移，再同步網址。");
        var snapshot = await LoadSnapshotAsync(site, cancellationToken, target);
        var serverId = snapshot.ServerId;
        if (snapshot.Bindings.Any(x => !string.Equals(x.SiteName, snapshot.SiteName, StringComparison.OrdinalIgnoreCase)
            && x.HostNames.Any(h => desired.Contains(h, StringComparer.OrdinalIgnoreCase))))
            throw new InvalidOperationException("網址已綁定其他 IIS 網站，請先處理衝突。");
        var pending = await db.ProvisioningTasks.AsNoTracking().Where(x => x.TargetServerId == serverId
            && x.Type == ProvisioningTaskType.SyncWebsiteBindings
            && (x.Status == ProvisioningTaskStatus.Pending || x.Status == ProvisioningTaskStatus.Running))
            .Select(x => x.PayloadJson).ToListAsync(cancellationToken);
        if (pending.Any(json => string.Equals(JsonSerializer.Deserialize<ProvisioningTaskPayload>(json)?.SiteName, snapshot.SiteName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("該站仍有網址同步任務等待或執行中，請完成後再修改。");
        return new ProvisioningTask
        {
            TargetServerId = serverId, Type = ProvisioningTaskType.SyncWebsiteBindings,
            Status = ProvisioningTaskStatus.Pending, CreatedAtUtc = DateTime.UtcNow,
            PayloadJson = JsonSerializer.Serialize(new ProvisioningTaskPayload(snapshot.SiteName, null, null, null, null,
                desired, PreviousHostNames: previous, PrimaryHostName: snapshot.Primary))
        };
    }

    private sealed record Snapshot(string ServerId, string Primary, string SiteName, DateTime LastSeenAtUtc,
        ProvisioningIisSiteBindingDto[] Bindings, string State, string[] IpAddresses);

    public sealed class SelectionRequiredException(IReadOnlyList<WebsiteBindingCandidate> candidates) : InvalidOperationException("請選擇目前提供服務的主機與網站。")
    {
        public IReadOnlyList<WebsiteBindingCandidate> Candidates { get; } = candidates;
    }

    private static WebsiteBindingComparison ToComparison(Snapshot snapshot) =>
        new("Ok", null, snapshot.SiteName, snapshot.LastSeenAtUtc,
            snapshot.Bindings.Where(x => string.Equals(x.SiteName, snapshot.SiteName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.HostNames).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToArray(),
            snapshot.Bindings.Where(x => !string.Equals(x.SiteName, snapshot.SiteName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.HostNames.Select(host => new IisBindingItemDto(host, x.SiteName))).ToArray());

    private async Task<Snapshot> LoadSnapshotAsync(Website site, CancellationToken cancellationToken, string? target = null)
    {
        var primary = PlatformDomainName.ToHost(site.DefaultUrl)
            ?? throw new InvalidOperationException("無法確認主要網址所屬的網站，請聯絡管理員。");
        // 網站歸屬取自受信任Worker的實際回報，不要求每個網站／主機位置再維護一份ServerId對應。
        var serverIds = (configuration.GetSection("Provisioning:Servers").Get<List<ProvisioningServerOptions>>() ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Id)).Select(x => x.Id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var threshold = DateTime.UtcNow.AddSeconds(-Math.Max(15, configuration.GetValue("Provisioning:AgentOfflineSeconds", 45)));
        var states = await db.ProvisioningAgentStatuses.AsNoTracking()
            .Where(x => serverIds.Contains(x.ServerId) && x.LastSeenAtUtc >= threshold)
            .Select(x => new { x.ServerId, x.LastSeenAtUtc, x.AppPoolMetricsJson }).ToListAsync(cancellationToken);
        if (states.Count == 0)
            throw new InvalidOperationException("目前沒有已設定且在線的 Worker 回報，無法確認 IIS 繫結；請檢查 Worker 連線及伺服器設定。");
        var matches = new List<Snapshot>();
        foreach (var state in states)
        {
            var pools = JsonSerializer.Deserialize<List<ProvisioningAppPoolMetricDto>>(state.AppPoolMetricsJson) ?? [];
            var bindings = pools.SelectMany(x => x.SiteBindings ?? []).Select(binding => binding with
            {
                HostNames = binding.HostNames.Select(PlatformDomainName.ToHost).OfType<string>()
                    .Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            }).ToArray();
            var names = bindings.Where(x => x.HostNames.Any(host => string.Equals(
                    PlatformDomainName.ToHost(host), primary, StringComparison.OrdinalIgnoreCase)))
                .Select(x => x.SiteName).Distinct(StringComparer.OrdinalIgnoreCase);
            foreach (var name in names)
                matches.Add(new Snapshot(state.ServerId, primary, name,
                    DateTime.SpecifyKind(state.LastSeenAtUtc, DateTimeKind.Utc), bindings,
                    bindings.First(x => x.SiteName == name).State,
                    pools.SelectMany(x => x.HostIpAddresses ?? []).Distinct().ToArray()));
        }
        if (matches.Count == 0)
            throw new InvalidOperationException($"在線 Worker 的 IIS 繫結回報中找不到主要網址 {primary}；請確認 Worker 版本、站台與應用程式集區關聯及網址設定。");
        if (!string.IsNullOrWhiteSpace(site.HostLocation))
        {
            if (!System.Net.IPAddress.TryParse(site.HostLocation.Trim(), out var configuredIp))
                throw new InvalidOperationException("目前主機位置不是 IP，無法同步網址。");
            matches = matches.Where(x => x.IpAddresses.Any(ip => System.Net.IPAddress.TryParse(ip, out var address)
                && address.MapToIPv6().Equals(configuredIp.MapToIPv6()))).ToList();
            if (matches.Count == 0)
                throw new InvalidOperationException($"已找到主要網址 {primary} 的 IIS 繫結，但 Worker 回報的主機 IP 不符合網站主機位置 {site.HostLocation.Trim()}；請確認主機位置與 Worker IP 回報（含 NAT 對應）。");
        }
        if (string.IsNullOrWhiteSpace(site.HostLocation))
        {
            var started = matches.Where(x => string.Equals(x.State, "Started", StringComparison.OrdinalIgnoreCase)).ToList();
            if (started.Count == 1) matches = started;
        }
        if (!string.IsNullOrEmpty(target))
        {
            var selected = matches.SingleOrDefault(x => Target(x) == target);
            if (selected == null) throw new InvalidOperationException("選擇的主機狀態已改變，請重新整理後選擇。");
            return selected;
        }
        if (matches.Count > 1)
        {
            var servers = configuration.GetSection("Provisioning:Servers").Get<List<ProvisioningServerOptions>>() ?? [];
            throw new SelectionRequiredException(matches.Select(x => new WebsiteBindingCandidate(Target(x),
                servers.FirstOrDefault(server => server.Id == x.ServerId)?.DisplayName ?? x.ServerId,
                x.SiteName, x.State, x.IpAddresses)).ToArray());
        }
        return matches[0];
    }

    private static string Target(Snapshot snapshot) => $"{snapshot.ServerId}\n{snapshot.SiteName}";
}
