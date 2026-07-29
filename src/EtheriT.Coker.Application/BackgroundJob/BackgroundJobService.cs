using Hangfire;
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
        public BackgroundJobService(IRecurringJobManager recurringJobManager)
        {
            _recurringJobManager = recurringJobManager;
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
            _recurringJobManager.RemoveIfExists("FlowSizes"); //暫時移除該工作
            //_recurringJobManager.AddOrUpdate<FlowSizesWorking>("FlowSizes", job => job.FlowSizeCollection(), Cron.Daily(17, 00));
        }
	}
}
