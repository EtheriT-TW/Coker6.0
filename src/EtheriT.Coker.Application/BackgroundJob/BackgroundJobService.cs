using Hangfire;
using EtheriT.Coker.Application.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.BackgroundJob
{
    public class BackgroundJobService
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly ECPayPaymentReconciliationOptions ecpayPaymentOptions;
        public BackgroundJobService(
            IRecurringJobManager recurringJobManager,
            IOptions<ECPayPaymentReconciliationOptions> ecpayPaymentOptions)
        {
            _recurringJobManager = recurringJobManager;
            this.ecpayPaymentOptions = ecpayPaymentOptions.Value;
        }
        public void InitializeJobs()
        {
            _recurringJobManager.AddOrUpdate<UserHabitsWorking>("UserHabits", job => job.HabitCollection(), Cron.Daily(18,30));
            _recurringJobManager.AddOrUpdate<ProductExportBackgroundJob>(
                "ProductBackgroundTaskCleanup",
                job => job.CleanupExpiredFiles(),
                Cron.Daily(3));
            _recurringJobManager.AddOrUpdate<LogCleanupWorking>(
                "LogCleanup",
                job => job.CleanupExpiredLogs(),
                Cron.Daily(4));
            _recurringJobManager.AddOrUpdate<DatabaseRetentionWorking>(
                "AuditLogRetentionCleanup",
                job => job.CleanupAuditLogs(),
                Cron.Daily(4, 30),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
            _recurringJobManager.AddOrUpdate<DatabaseRetentionWorking>(
                "ExpiredTokenCleanup",
                job => job.CleanupExpiredTokens(),
                Cron.Hourly(15),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
            _recurringJobManager.AddOrUpdate<DatabaseRetentionWorking>(
                "HangfireFailedJobRetentionCleanup",
                job => job.CleanupExpiredHangfireFailedJobs(),
                Cron.Daily(5),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
            _recurringJobManager.AddOrUpdate<FileCleanupWorking>(
                "FileReferenceScan",
                job => job.ScanAllWebsitesAsync(),
                Cron.Daily(3, 30),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
            _recurringJobManager.AddOrUpdate<FileCleanupWorking>(
                "FileRecycleBinCleanup",
                job => job.PurgeExpiredRecycleBinAsync(),
                Cron.Daily(4, 15),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
            _recurringJobManager.AddOrUpdate<RemoteDailyStatisticsWorking>(
                "RemoteDailyStatistics",
                job => job.AggregateNextDay(),
                "5,20,35,50 * * * *",
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
            _recurringJobManager.AddOrUpdate<RemoteRetentionWorking>(
                "RemoteRetentionCleanup",
                job => job.CleanupAggregatedRemotes(),
                Cron.Hourly(0),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
            _recurringJobManager.AddOrUpdate<PageTextBackfillJob>(
                "PageTextBackfill",
                job => job.Run(),
                Cron.Daily(2));
            _recurringJobManager.AddOrUpdate<HtmlLegacyAttributeNormalizationJob>(
                "HtmlLegacyAttributeNormalization",
                job => job.Run(),
                Cron.Daily(2, 30));
            _recurringJobManager.AddOrUpdate<CdnProviderIpRangeSyncJob>(
                "CdnProviderIpRangeSync",
                job => job.SynchronizeAllAsync(),
                Cron.Daily(1),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
            if (ecpayPaymentOptions.Enabled)
            {
                var intervalMinutes = Math.Clamp(
                    ecpayPaymentOptions.IntervalMinutes,
                    1,
                    60);
                var schedule = intervalMinutes == 60
                    ? Cron.Hourly()
                    : Cron.MinuteInterval(intervalMinutes);

                _recurringJobManager.AddOrUpdate<ECPayPaymentReconciliationWorking>(
                    "ECPayPaymentReconciliation",
                    job => job.ReconcilePendingApplePayOrders(),
                    schedule,
                    new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
            }
            else
            {
                _recurringJobManager.RemoveIfExists("ECPayPaymentReconciliation");
            }
            _recurringJobManager.RemoveIfExists("FlowSizes"); //暫時移除該工作
            //_recurringJobManager.AddOrUpdate<FlowSizesWorking>("FlowSizes", job => job.FlowSizeCollection(), Cron.Daily(17, 00));
        }
	}
}
