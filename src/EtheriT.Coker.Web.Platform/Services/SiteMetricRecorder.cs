using System.Text.Json;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.Provisioning;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Platform.Services;

public sealed class SiteMetricRecorder(CokerDbContext db, IConfiguration configuration, ILogger<SiteMetricRecorder> logger)
{
    public bool Enabled => configuration.GetValue("Provisioning:Monitoring:SiteHistoryEnabled", false);

    public async Task RecordAsync(string serverId, DateTime nowUtc,
        IReadOnlyList<ProvisioningAppPoolMetricDto> pools, CancellationToken cancellationToken)
    {
        if (!Enabled) return;
        var added = new List<ProvisioningSiteMetricSample>();
        try
        {
            var websites = await db.Websites.AsNoTracking().Where(w => !w.IsDeleted)
                .Select(w => new { w.Id, w.OrgName, w.Title, w.DefaultUrl }).ToListAsync(cancellationToken);
            var byHost = websites.Select(w => new { Host = PlatformDomainName.ToHost(w.DefaultUrl), Site = w })
                .Where(w => w.Host != null).ToLookup(w => w.Host!, w => w.Site, StringComparer.OrdinalIgnoreCase);
            var mapped = pools.Where(p => !string.IsNullOrWhiteSpace(p.ApplicationPoolName))
                .GroupBy(p => p.ApplicationPoolName, StringComparer.OrdinalIgnoreCase).Select(g => g.First())
                .Select(pool => new
                {
                    Pool = pool,
                    Sites = (pool.HostNames ?? []).Select(PlatformDomainName.ToHost).Where(h => h != null)
                        .SelectMany(h => byHost[h!]).DistinctBy(w => w.Id).OrderBy(w => w.Id).ToArray()
                }).ToArray();
            var siteIds = mapped.SelectMany(p => p.Sites).Select(w => w.Id).Distinct().ToArray();
            Dictionary<long, long>? online = null;
            try
            {
                // 與Dashboard一致：最近兩分鐘心跳，登入user去重，其餘以UUID去重。
                online = await db.Remotes.AsNoTracking()
                    .Where(r => siteIds.Contains(r.FK_WebsiteId) && r.TrackingEventId != null
                        && r.LastHeartbeatAt >= DateTime.Now.AddMinutes(-2)
                        && (r.FK_UserId != null || r.UUID != Guid.Empty))
                    .Select(r => new { r.FK_WebsiteId, r.FK_UserId, UUID = r.FK_UserId != null ? Guid.Empty : r.UUID })
                    .Distinct().GroupBy(r => r.FK_WebsiteId)
                    .Select(g => new { WebsiteId = g.Key, Count = g.LongCount() })
                    .ToDictionaryAsync(g => g.WebsiteId, g => g.Count, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // 人數讀取失敗不能寫成0，也不能中斷Worker心跳。
                logger.LogWarning(ex, "Unable to sample online visitors for {ServerId}", serverId);
            }

            foreach (var item in mapped)
            {
                var pool = item.Pool;
                added.Add(new ProvisioningSiteMetricSample
                {
                    ServerId = serverId, ApplicationPoolName = pool.ApplicationPoolName,
                    SampledAtUtc = nowUtc, PrivateMemoryBytes = Math.Max(0, pool.PrivateMemoryBytes),
                    WorkingSetBytes = Math.Max(0, pool.WorkingSetBytes), CpuUsagePercent = pool.CpuUsagePercent,
                    OnlineVisitors = online != null && item.Sites.Length > 0
                        ? item.Sites.Sum(site => online.GetValueOrDefault(site.Id)) : null,
                    WebsiteIdsJson = JsonSerializer.Serialize(item.Sites.Select(w => w.Id)),
                    WebsiteNamesJson = JsonSerializer.Serialize(item.Sites.Select(w => $"{w.OrgName}｜{w.Title}")),
                    ProcessIdsJson = JsonSerializer.Serialize(pool.ProcessIds.OrderBy(id => id))
                });
            }
            db.ProvisioningSiteMetricSamples.AddRange(added);
            await db.SaveChangesAsync(cancellationToken);
            var retentionDays = Math.Clamp(configuration.GetValue("Provisioning:HistoryRetentionDays", 30), 1, 365);
            var expires = nowUtc.AddDays(-retentionDays);
            await db.ProvisioningSiteMetricSamples.Where(s => s.SampledAtUtc < expires).ExecuteDeleteAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            foreach (var row in added) db.Entry(row).State = EntityState.Detached;
            logger.LogWarning(ex, "Unable to save site history for {ServerId}", serverId);
        }
    }
}
