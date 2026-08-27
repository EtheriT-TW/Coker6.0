using System.Data;
using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Application.BackgroundJob
{
    public sealed class RemoteDailyStatisticsWorking
    {
        internal const int CurrentAggregationVersion = 2;

        private readonly CokerDbContext db;
        private readonly RemoteAnalyticsOptions options;
        private readonly ILogger<RemoteDailyStatisticsWorking> logger;

        public RemoteDailyStatisticsWorking(
            CokerDbContext db,
            IOptions<RemoteAnalyticsOptions> options,
            ILogger<RemoteDailyStatisticsWorking> logger)
        {
            this.db = db;
            this.options = options.Value;
            this.logger = logger;
        }

        [AutomaticRetry(Attempts = 1)]
        [DisableConcurrentExecution(3600)]
        public async Task AggregateNextDay()
        {
            var today = DateTime.Today;
            var latestClosedDate = DateTime.Today.AddDays(-1);
            var earliestRemoteTime = await db.Remotes
                .AsNoTracking()
                .OrderBy(remote => remote.ExecutionTime)
                .Select(remote => (DateTime?)remote.ExecutionTime)
                .FirstOrDefaultAsync();

            if (!earliestRemoteTime.HasValue)
            {
                logger.LogInformation("Remote daily aggregation skipped because no Remote records exist.");
                return;
            }

            var earliestDate = earliestRemoteTime.Value.Date;
            var todaySourceRows = await AggregateDate(today, markComplete: false);
            logger.LogInformation(
                "Current Remote daily statistics refreshed. StatisticDate={StatisticDate}, SourceRows={SourceRows}",
                today,
                todaySourceRows);

            if (earliestDate > latestClosedDate)
            {
                logger.LogInformation("Remote daily aggregation skipped because no closed day is available.");
                return;
            }

            var completedDates = (await db.RemoteDailyAggregationRuns
                    .AsNoTracking()
                    .Where(run =>
                        run.StatisticDate >= earliestDate
                        && run.StatisticDate <= latestClosedDate
                        && run.AggregationVersion == CurrentAggregationVersion)
                    .Select(run => run.StatisticDate)
                    .ToListAsync())
                .Select(date => date.Date)
                .ToHashSet();

            DateTime? targetDate = null;
            for (var date = latestClosedDate; date >= earliestDate; date = date.AddDays(-1))
            {
                if (!completedDates.Contains(date))
                {
                    targetDate = date;
                    break;
                }
            }

            if (!targetDate.HasValue)
            {
                logger.LogInformation(
                    "Remote daily aggregation is up to date through {LatestClosedDate}.",
                    latestClosedDate);
                return;
            }

            var sourceRows = await AggregateDate(targetDate.Value, markComplete: true);
            logger.LogInformation(
                "Remote daily aggregation completed. StatisticDate={StatisticDate}, SourceRows={SourceRows}",
                targetDate.Value,
                sourceRows);
        }

        private async Task<long> AggregateDate(DateTime statisticDate, bool markComplete)
        {
            const string commandText =
                """
                SET NOCOUNT ON;
                SET XACT_ABORT ON;

                DECLARE @NextDate date = DATEADD(day, 1, @StatisticDate);
                DECLARE @AggregatedAt datetime2 = GETDATE();
                DECLARE @SourceRows bigint =
                (
                    SELECT COUNT_BIG(*)
                    FROM [dbo].[Remotes]
                    WHERE [ExecutionTime] >= @StatisticDate
                      AND [ExecutionTime] < @NextDate
                );

                BEGIN TRANSACTION;

                DELETE FROM [dbo].[RemoteDailyStatistics]
                WHERE [StatisticDate] = @StatisticDate;

                DELETE FROM [dbo].[RemoteHourlyStatistics]
                WHERE [StatisticHour] >= @StatisticDate
                  AND [StatisticHour] < @NextDate;

                ;WITH [Source] AS
                (
                    SELECT
                        DATEADD(
                            hour,
                            DATEDIFF(hour, CONVERT(datetime2, '20000101'), [remote].[ExecutionTime]),
                            CONVERT(datetime2, '20000101')
                        ) AS [StatisticHour],
                        [remote].[FK_WebsiteId],
                        CASE
                            WHEN [remote].[FK_UserId] IS NOT NULL
                                THEN CONCAT('user:', [remote].[FK_UserId])
                            WHEN [remote].[UUID] <> '00000000-0000-0000-0000-000000000000'
                                THEN CONCAT('uuid:', CONVERT(varchar(36), [remote].[UUID]))
                            ELSE CONCAT('ip:', ISNULL([remote].[ClientIpAddress], ''))
                        END AS [VisitorIdentifier],
                        CASE
                            WHEN [remote].[IsEngaged] = CAST(1 AS bit)
                              OR
                              (
                                  [remote].[TrackingEventId] IS NULL
                                  AND [remote].[TimeOnPage] > 0
                                  AND [remote].[State] <> 2
                              )
                                THEN 1
                            ELSE 0
                        END AS [IsEffective],
                        CASE
                            WHEN [remote].[TrackingEventId] IS NULL
                             AND [remote].[TimeOnPage] > 0
                             AND [remote].[State] <> 2
                                THEN 1
                            ELSE 0
                        END AS [IsLegacy],
                        CASE
                            WHEN [remote].[TimeOnPage] > 0 THEN [remote].[TimeOnPage]
                            ELSE 0
                        END AS [VisibleSeconds]
                    FROM [dbo].[Remotes] AS [remote]
                    WHERE [remote].[ExecutionTime] >= @StatisticDate
                      AND [remote].[ExecutionTime] < @NextDate
                )
                INSERT INTO [dbo].[RemoteHourlyStatistics]
                (
                    [StatisticHour],
                    [FK_WebsiteId],
                    [PageViews],
                    [EffectiveViews],
                    [LegacyViews],
                    [UniqueVisitors],
                    [EffectiveUniqueVisitors],
                    [TotalVisibleSeconds],
                    [AggregatedAt]
                )
                SELECT
                    [source].[StatisticHour],
                    [source].[FK_WebsiteId],
                    COUNT_BIG(*),
                    SUM(CONVERT(bigint, [source].[IsEffective])),
                    SUM(CONVERT(bigint, [source].[IsLegacy])),
                    COUNT_BIG(DISTINCT [source].[VisitorIdentifier]),
                    COUNT_BIG(DISTINCT CASE
                        WHEN [source].[IsEffective] = 1 THEN [source].[VisitorIdentifier]
                    END),
                    SUM(CONVERT(bigint, CASE
                        WHEN [source].[IsEffective] = 1 THEN [source].[VisibleSeconds]
                        ELSE 0
                    END)),
                    @AggregatedAt
                FROM [Source] AS [source]
                GROUP BY
                    [source].[StatisticHour],
                    [source].[FK_WebsiteId];

                ;WITH [Source] AS
                (
                    SELECT
                        [remote].[FK_WebsiteId],
                        [remote].[FK_WebmenuId],
                        [remote].[FK_ArticleId],
                        [remote].[FK_ProdId],
                        [remote].[FK_TechCertId],
                        CASE
                            WHEN [remote].[FK_UserId] IS NOT NULL
                                THEN CONCAT('user:', [remote].[FK_UserId])
                            WHEN [remote].[UUID] <> '00000000-0000-0000-0000-000000000000'
                                THEN CONCAT('uuid:', CONVERT(varchar(36), [remote].[UUID]))
                            ELSE CONCAT('ip:', ISNULL([remote].[ClientIpAddress], ''))
                        END AS [VisitorIdentifier],
                        CASE
                            WHEN [remote].[IsEngaged] = CAST(1 AS bit)
                              OR
                              (
                                  [remote].[TrackingEventId] IS NULL
                                  AND [remote].[TimeOnPage] > 0
                                  AND [remote].[State] <> 2
                              )
                                THEN 1
                            ELSE 0
                        END AS [IsEffective],
                        CASE
                            WHEN [remote].[TrackingEventId] IS NULL
                             AND [remote].[TimeOnPage] > 0
                             AND [remote].[State] <> 2
                                THEN 1
                            ELSE 0
                        END AS [IsLegacy],
                        CASE
                            WHEN [remote].[TimeOnPage] > 0 THEN [remote].[TimeOnPage]
                            ELSE 0
                        END AS [VisibleSeconds]
                    FROM [dbo].[Remotes] AS [remote]
                    WHERE [remote].[ExecutionTime] >= @StatisticDate
                      AND [remote].[ExecutionTime] < @NextDate
                )
                INSERT INTO [dbo].[RemoteDailyStatistics]
                (
                    [StatisticDate],
                    [FK_WebsiteId],
                    [Scope],
                    [FK_WebmenuId],
                    [FK_ArticleId],
                    [FK_ProdId],
                    [FK_TechCertId],
                    [PageViews],
                    [EffectiveViews],
                    [LegacyViews],
                    [UniqueVisitors],
                    [EffectiveUniqueVisitors],
                    [TotalVisibleSeconds],
                    [AggregatedAt]
                )
                SELECT
                    @StatisticDate,
                    [source].[FK_WebsiteId],
                    0,
                    0,
                    0,
                    0,
                    0,
                    COUNT_BIG(*),
                    SUM(CONVERT(bigint, [source].[IsEffective])),
                    SUM(CONVERT(bigint, [source].[IsLegacy])),
                    COUNT_BIG(DISTINCT [source].[VisitorIdentifier]),
                    COUNT_BIG(DISTINCT CASE
                        WHEN [source].[IsEffective] = 1 THEN [source].[VisitorIdentifier]
                    END),
                    SUM(CONVERT(bigint, CASE
                        WHEN [source].[IsEffective] = 1 THEN [source].[VisibleSeconds]
                        ELSE 0
                    END)),
                    @AggregatedAt
                FROM [Source] AS [source]
                GROUP BY [source].[FK_WebsiteId];

                ;WITH [Source] AS
                (
                    SELECT
                        [remote].[FK_WebsiteId],
                        [remote].[FK_WebmenuId],
                        ISNULL([remote].[FK_ArticleId], 0) AS [FK_ArticleId],
                        ISNULL([remote].[FK_ProdId], 0) AS [FK_ProdId],
                        ISNULL([remote].[FK_TechCertId], 0) AS [FK_TechCertId],
                        CASE
                            WHEN [remote].[FK_UserId] IS NOT NULL
                                THEN CONCAT('user:', [remote].[FK_UserId])
                            WHEN [remote].[UUID] <> '00000000-0000-0000-0000-000000000000'
                                THEN CONCAT('uuid:', CONVERT(varchar(36), [remote].[UUID]))
                            ELSE CONCAT('ip:', ISNULL([remote].[ClientIpAddress], ''))
                        END AS [VisitorIdentifier],
                        CASE
                            WHEN [remote].[IsEngaged] = CAST(1 AS bit)
                              OR
                              (
                                  [remote].[TrackingEventId] IS NULL
                                  AND [remote].[TimeOnPage] > 0
                                  AND [remote].[State] <> 2
                              )
                                THEN 1
                            ELSE 0
                        END AS [IsEffective],
                        CASE
                            WHEN [remote].[TrackingEventId] IS NULL
                             AND [remote].[TimeOnPage] > 0
                             AND [remote].[State] <> 2
                                THEN 1
                            ELSE 0
                        END AS [IsLegacy],
                        CASE
                            WHEN [remote].[TimeOnPage] > 0 THEN [remote].[TimeOnPage]
                            ELSE 0
                        END AS [VisibleSeconds]
                    FROM [dbo].[Remotes] AS [remote]
                    WHERE [remote].[ExecutionTime] >= @StatisticDate
                      AND [remote].[ExecutionTime] < @NextDate
                )
                INSERT INTO [dbo].[RemoteDailyStatistics]
                (
                    [StatisticDate],
                    [FK_WebsiteId],
                    [Scope],
                    [FK_WebmenuId],
                    [FK_ArticleId],
                    [FK_ProdId],
                    [FK_TechCertId],
                    [PageViews],
                    [EffectiveViews],
                    [LegacyViews],
                    [UniqueVisitors],
                    [EffectiveUniqueVisitors],
                    [TotalVisibleSeconds],
                    [AggregatedAt]
                )
                SELECT
                    @StatisticDate,
                    [source].[FK_WebsiteId],
                    1,
                    [source].[FK_WebmenuId],
                    [source].[FK_ArticleId],
                    [source].[FK_ProdId],
                    [source].[FK_TechCertId],
                    COUNT_BIG(*),
                    SUM(CONVERT(bigint, [source].[IsEffective])),
                    SUM(CONVERT(bigint, [source].[IsLegacy])),
                    COUNT_BIG(DISTINCT [source].[VisitorIdentifier]),
                    COUNT_BIG(DISTINCT CASE
                        WHEN [source].[IsEffective] = 1 THEN [source].[VisitorIdentifier]
                    END),
                    SUM(CONVERT(bigint, CASE
                        WHEN [source].[IsEffective] = 1 THEN [source].[VisibleSeconds]
                        ELSE 0
                    END)),
                    @AggregatedAt
                FROM [Source] AS [source]
                GROUP BY
                    [source].[FK_WebsiteId],
                    [source].[FK_WebmenuId],
                    [source].[FK_ArticleId],
                    [source].[FK_ProdId],
                    [source].[FK_TechCertId];

                IF @MarkComplete = CAST(1 AS bit)
                BEGIN
                    UPDATE [run]
                    SET
                        [run].[SourceRows] = @SourceRows,
                        [run].[AggregationVersion] = @AggregationVersion,
                        [run].[CompletedAt] = @AggregatedAt
                    FROM [dbo].[RemoteDailyAggregationRuns] AS [run]
                    WHERE [run].[StatisticDate] = @StatisticDate;

                    IF @@ROWCOUNT = 0
                    BEGIN
                        INSERT INTO [dbo].[RemoteDailyAggregationRuns]
                        (
                            [StatisticDate],
                            [AggregationVersion],
                            [SourceRows],
                            [CompletedAt]
                        )
                        VALUES
                        (
                            @StatisticDate,
                            @AggregationVersion,
                            @SourceRows,
                            @AggregatedAt
                        );
                    END;
                END;

                ;WITH [Popular] AS
                (
                    SELECT
                        [stat].[FK_WebmenuId] AS [TargetId],
                        SUM([stat].[EffectiveViews]) AS [TotalViews]
                    FROM [dbo].[RemoteDailyStatistics] AS [stat]
                    WHERE [stat].[Scope] = 1
                      AND [stat].[FK_WebmenuId] > 0
                    GROUP BY [stat].[FK_WebmenuId]
                )
                UPDATE [menu]
                SET [menu].[Popular] = CONVERT(int, CASE
                    WHEN ISNULL([popular].[TotalViews], 0) > 2147483647 THEN 2147483647
                    ELSE ISNULL([popular].[TotalViews], 0)
                END)
                FROM [dbo].[WebMenus] AS [menu]
                LEFT JOIN [Popular] AS [popular]
                    ON [popular].[TargetId] = [menu].[Id]
                WHERE [menu].[Popular] <> CONVERT(int, CASE
                    WHEN ISNULL([popular].[TotalViews], 0) > 2147483647 THEN 2147483647
                    ELSE ISNULL([popular].[TotalViews], 0)
                END);

                ;WITH [Popular] AS
                (
                    SELECT
                        [stat].[FK_ArticleId] AS [TargetId],
                        SUM([stat].[EffectiveViews]) AS [TotalViews]
                    FROM [dbo].[RemoteDailyStatistics] AS [stat]
                    WHERE [stat].[Scope] = 1
                      AND [stat].[FK_ArticleId] > 0
                    GROUP BY [stat].[FK_ArticleId]
                )
                UPDATE [article]
                SET [article].[Popular] = CONVERT(int, CASE
                    WHEN ISNULL([popular].[TotalViews], 0) > 2147483647 THEN 2147483647
                    ELSE ISNULL([popular].[TotalViews], 0)
                END)
                FROM [dbo].[Article] AS [article]
                LEFT JOIN [Popular] AS [popular]
                    ON [popular].[TargetId] = [article].[Id]
                WHERE [article].[Popular] <> CONVERT(int, CASE
                    WHEN ISNULL([popular].[TotalViews], 0) > 2147483647 THEN 2147483647
                    ELSE ISNULL([popular].[TotalViews], 0)
                END);

                ;WITH [Popular] AS
                (
                    SELECT
                        [stat].[FK_ProdId] AS [TargetId],
                        SUM([stat].[EffectiveViews]) AS [TotalViews]
                    FROM [dbo].[RemoteDailyStatistics] AS [stat]
                    WHERE [stat].[Scope] = 1
                      AND [stat].[FK_ProdId] > 0
                    GROUP BY [stat].[FK_ProdId]
                )
                UPDATE [prod]
                SET [prod].[Popular] = CONVERT(int, CASE
                    WHEN ISNULL([popular].[TotalViews], 0) > 2147483647 THEN 2147483647
                    ELSE ISNULL([popular].[TotalViews], 0)
                END)
                FROM [dbo].[Prods] AS [prod]
                LEFT JOIN [Popular] AS [popular]
                    ON [popular].[TargetId] = [prod].[Id]
                WHERE [prod].[Popular] <> CONVERT(int, CASE
                    WHEN ISNULL([popular].[TotalViews], 0) > 2147483647 THEN 2147483647
                    ELSE ISNULL([popular].[TotalViews], 0)
                END);

                COMMIT TRANSACTION;

                SELECT @SourceRows;
                """;

            var connection = db.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;
            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = commandText;
                command.CommandTimeout = Math.Clamp(options.CommandTimeoutSeconds, 60, 3_600);

                var dateParameter = command.CreateParameter();
                dateParameter.ParameterName = "@StatisticDate";
                dateParameter.Value = statisticDate.Date;
                command.Parameters.Add(dateParameter);

                var markCompleteParameter = command.CreateParameter();
                markCompleteParameter.ParameterName = "@MarkComplete";
                markCompleteParameter.Value = markComplete;
                command.Parameters.Add(markCompleteParameter);

                var versionParameter = command.CreateParameter();
                versionParameter.ParameterName = "@AggregationVersion";
                versionParameter.Value = CurrentAggregationVersion;
                command.Parameters.Add(versionParameter);

                var result = await command.ExecuteScalarAsync();
                return result == null || result == DBNull.Value
                    ? 0
                    : Convert.ToInt64(result);
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }
    }
}
