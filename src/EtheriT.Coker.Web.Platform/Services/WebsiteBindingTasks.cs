using System.Text.Json;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.Provisioning;
using EtheriT.Coker.Web.Platform.Models.Websites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Web.Platform.Services;

public sealed class WebsiteBindingTasks(CokerDbContext db, IOptions<WebsiteHostingOptions> hosting, IConfiguration configuration)
{
    public async Task<WebsiteBindingComparison> ImportExistingUrlsAsync(Website site, CancellationToken cancellationToken)
    {
        var snapshot = await LoadSnapshotAsync(site, cancellationToken);
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

    public async Task<WebsiteBindingComparison> CompareAsync(Website site, CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = await LoadSnapshotAsync(site, cancellationToken);
            return ToComparison(snapshot);
        }
        catch (Exception ex) when (ex is InvalidOperationException or JsonException)
        {
            return new WebsiteBindingComparison("Unavailable", ex is JsonException ? "Worker 繫結資料無法辨識。" : ex.Message,
                null, null, [], []);
        }
    }

    public async Task<ProvisioningTask> CreateAsync(Website site, string? newLocation,
        IReadOnlyList<string> previous, IReadOnlyList<string> desired, CancellationToken cancellationToken)
    {
        if (site.HostLocation != newLocation?.Trim())
            throw new InvalidOperationException("網址與主機位置不能同時變更；請先完成網站搬移，再同步網址。");
        var snapshot = await LoadSnapshotAsync(site, cancellationToken);
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
        ProvisioningIisSiteBindingDto[] Bindings);

    private static WebsiteBindingComparison ToComparison(Snapshot snapshot) =>
        new("Ok", null, snapshot.SiteName, snapshot.LastSeenAtUtc,
            snapshot.Bindings.Where(x => string.Equals(x.SiteName, snapshot.SiteName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.HostNames).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToArray(),
            snapshot.Bindings.Where(x => !string.Equals(x.SiteName, snapshot.SiteName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.HostNames.Select(host => new IisBindingItemDto(host, x.SiteName))).ToArray());

    private async Task<Snapshot> LoadSnapshotAsync(Website site, CancellationToken cancellationToken)
    {
        var host = hosting.Value.HostLocations.SingleOrDefault(x => x.Value == site.HostLocation);
        if (host is not { HasWorker: true }) throw new InvalidOperationException("此主機位置未提供網址管理服務，請聯絡管理員。");
        var serverId = host.ServerId!.Trim();
        var state = await db.ProvisioningAgentStatuses.AsNoTracking().SingleOrDefaultAsync(x => x.ServerId == serverId, cancellationToken);
        if (state == null || state.LastSeenAtUtc < DateTime.UtcNow.AddSeconds(-Math.Max(15, configuration.GetValue("Provisioning:AgentOfflineSeconds", 45))))
            throw new InvalidOperationException("目前無法取得主機的最新狀態，請稍後再儲存網址。");
        var primary = PlatformDomainName.ToHost(site.DefaultUrl)
            ?? throw new InvalidOperationException("無法確認主要網址所屬的網站，請聯絡管理員。");
        var pools = JsonSerializer.Deserialize<List<ProvisioningAppPoolMetricDto>>(state.AppPoolMetricsJson) ?? [];
        var bindings = pools.SelectMany(x => x.SiteBindings ?? []).ToArray();
        var targets = bindings.Where(x => x.HostNames.Contains(primary, StringComparer.OrdinalIgnoreCase))
            .Select(x => x.SiteName).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (targets.Length != 1) throw new InvalidOperationException("無法確認此網站在主機上的網址設定，請聯絡管理員。");
        return new Snapshot(serverId, primary, targets[0], DateTime.SpecifyKind(state.LastSeenAtUtc, DateTimeKind.Utc), bindings);
    }
}
