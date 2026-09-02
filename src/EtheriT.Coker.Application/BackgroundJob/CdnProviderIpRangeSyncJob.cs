using EtheriT.Coker.Application.Cdn;
using EtheriT.Coker.Application.Shared.Cdn;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EtheriT.Coker.Application.BackgroundJob
{
    public sealed class CdnProviderIpRangeSyncJob
    {
        private readonly CokerDbContext db;
        private readonly CdnProviderIpRangeDownloader downloader;
        private readonly CdnProviderSyncAlertService alertService;
        private readonly ILogger<CdnProviderIpRangeSyncJob> logger;

        public CdnProviderIpRangeSyncJob(
            CokerDbContext db,
            CdnProviderIpRangeDownloader downloader,
            CdnProviderSyncAlertService alertService,
            ILogger<CdnProviderIpRangeSyncJob> logger)
        {
            this.db = db;
            this.downloader = downloader;
            this.alertService = alertService;
            this.logger = logger;
        }

        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(3600)]
        public async Task SynchronizeAllAsync()
        {
            var failures = new List<Exception>();

            foreach (var provider in CdnProviderDefinitions.All.OrderBy(x => x.Key))
            {
                if (!provider.SupportsAutomaticSync)
                {
                    logger.LogInformation(
                        "CDN IP range synchronization skipped. Provider={Provider}, Reason={Reason}",
                        provider.Key,
                        provider.AutomaticSyncUnavailableReason);
                    continue;
                }

                var failure = await SynchronizeProviderSafelyAsync(provider);
                if (failure != null)
                    failures.Add(failure);
            }

            if (failures.Count > 0)
                throw new AggregateException("一個或多個 CDN IP 清單同步失敗。", failures);
        }

        private async Task<Exception?> SynchronizeProviderSafelyAsync(CdnProviderDefinition provider)
        {
            try
            {
                var ranges = await downloader.DownloadAsync(provider);
                await SaveSuccessfulSyncAsync(provider.Key, ranges);
                return null;
            }
            catch (Exception ex)
            {
                db.ChangeTracker.Clear();
                await SaveFailedSyncAsync(provider.Key, ex);

                logger.LogError(
                    ex,
                    "CDN IP range synchronization failed. Provider={Provider}",
                    provider.Key);

                return ex;
            }
        }

        private async Task SaveSuccessfulSyncAsync(
            string provider,
                    IReadOnlyCollection<CdnDownloadedIpRange> downloadedRanges)
        {
            var now = DateTime.UtcNow;

            var storedRows = await db.CdnProviderIpRanges
                .IgnoreQueryFilters()
                .Where(x => x.Provider == provider)
                .ToListAsync();

            var storedByCidr = storedRows
                .GroupBy(x => x.Cidr, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(x => x.IsDeleted)
                        .ThenByDescending(x => x.Id)
                        .First(),
                    StringComparer.OrdinalIgnoreCase);

            var incomingCidrs = downloadedRanges
                .Select(x => x.Cidr)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var addedCount = 0;
            var removedCount = 0;
            foreach (var downloaded in downloadedRanges)
            {
                if (storedByCidr.TryGetValue(downloaded.Cidr, out var stored))
                {
                    stored.IpVersion = downloaded.IpVersion;
                    stored.LastVerifiedTime = now;
                    stored.IsDeleted = false;
                    stored.DeletionTime = null;
                    stored.DeleterUserId = null;
                    stored.LastModifierUserId = 0;
                    stored.LastModificationTime = now;
                    continue;
                }

                db.CdnProviderIpRanges.Add(new CdnProviderIpRange
                {
                    Provider = provider,
                    Cidr = downloaded.Cidr,
                    IpVersion = downloaded.IpVersion,
                    LastVerifiedTime = now,
                    CreatorUserId = 0,
                    CreationTime = now
                });
                addedCount++;
            }

            foreach (var stored in storedRows.Where(x => !x.IsDeleted && !incomingCidrs.Contains(x.Cidr)))
            {
                stored.IsDeleted = true;
                stored.DeleterUserId = 0;
                stored.DeletionTime = now;
                stored.LastModifierUserId = 0;
                stored.LastModificationTime = now;
                removedCount++;
            }

            var state = await GetOrCreateSyncStateAsync(provider, now);
            state.LastAttemptTime = now;
            state.LastSuccessTime = now;
            state.ConsecutiveFailureCount = 0;
            state.LastError = null;
            state.AlertSent = false;
            state.LastAlertTime = null;
            state.LastModifierUserId = 0;
            state.LastModificationTime = now;

            await db.SaveChangesAsync();

            logger.LogInformation(
                "CDN IP range synchronization completed. Provider={Provider}, Total={Total}, Added={Added}, Removed={Removed}",
                provider,
                downloadedRanges.Count,
                addedCount,
                removedCount);
        }

        private async Task SaveFailedSyncAsync(string provider, Exception exception)
        {
            var now = DateTime.UtcNow;
            var state = await GetOrCreateSyncStateAsync(provider, now);
            var failedAttempts = exception is CdnIpSourceDownloadException downloadException
                ? downloadException.Attempts
                : 1;

            state.LastAttemptTime = now;
            state.ConsecutiveFailureCount += failedAttempts;
            state.LastError = Truncate(GetErrorMessage(exception), 4000);
            state.LastModifierUserId = 0;
            state.LastModificationTime = now;

            await db.SaveChangesAsync();

            if (state.ConsecutiveFailureCount < 3 || state.AlertSent)
                return;

            if (await alertService.SendAsync(
                    provider,
                    state.ConsecutiveFailureCount,
                    state.LastError))
            {
                state.AlertSent = true;
                state.LastAlertTime = DateTime.UtcNow;
                state.LastModifierUserId = 0;
                state.LastModificationTime = state.LastAlertTime;
                await db.SaveChangesAsync();
            }
        }

        private async Task<CdnProviderSyncState> GetOrCreateSyncStateAsync(
            string provider,
            DateTime now)
        {
            var state = await db.CdnProviderSyncStates
                .IgnoreQueryFilters()
                .Where(x => x.Provider == provider)
                .OrderBy(x => x.IsDeleted)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (state == null)
            {
                state = new CdnProviderSyncState
                {
                    Provider = provider,
                    CreatorUserId = 0,
                    CreationTime = now
                };
                db.CdnProviderSyncStates.Add(state);
            }
            else if (state.IsDeleted)
            {
                state.IsDeleted = false;
                state.DeletionTime = null;
                state.DeleterUserId = null;
            }

            return state;
        }

        private static string Truncate(string value, int maximumLength)
        {
            return value.Length <= maximumLength
                ? value
                : value[..maximumLength];
        }

        private static string GetErrorMessage(Exception exception)
        {
            var baseMessage = exception.GetBaseException().Message;
            return string.Equals(exception.Message, baseMessage, StringComparison.Ordinal)
                ? exception.Message
                : $"{exception.Message} 原因：{baseMessage}";
        }
    }
}
