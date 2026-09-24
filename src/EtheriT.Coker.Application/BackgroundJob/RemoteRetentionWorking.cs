using System.Data;
using System.Diagnostics;
using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Application.BackgroundJob
{
    public sealed class RemoteRetentionWorking
    {
        private readonly CokerDbContext db;
        private readonly DatabaseRetentionOptions options;
        private readonly ILogger<RemoteRetentionWorking> logger;

        public RemoteRetentionWorking(
            CokerDbContext db,
            IOptions<DatabaseRetentionOptions> options,
            ILogger<RemoteRetentionWorking> logger)
        {
            this.db = db;
            this.options = options.Value;
            this.logger = logger;
        }

        // 此工作每小時會再次執行；SQL 逾時時立即重試只會讓同一批查詢
        // 連續占用資料庫兩次，不交由 Hangfire 自動重試。
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(3600)]
        public async Task CleanupAggregatedRemotes()
        {
            var retentionDays = Math.Max(1, options.RemoteRetentionDays);
            var batchSize = Math.Clamp(options.RemoteBatchSize, 100, 5_000);
            var maxRows = Math.Max(batchSize, options.RemoteMaxRowsPerRun);
            var maxRunDuration = TimeSpan.FromSeconds(
                Math.Clamp(options.RemoteMaxRunSeconds, 5, 300));
            var cutoffDate = DateTime.Today.AddDays(-retentionDays);
            var totalDeleted = 0;
            var stopwatch = Stopwatch.StartNew();

            while (totalDeleted < maxRows && stopwatch.Elapsed < maxRunDuration)
            {
                var currentBatchSize = Math.Min(batchSize, maxRows - totalDeleted);
                var deleted = await DeleteBatch(
                    currentBatchSize,
                    cutoffDate,
                    RemoteDailyStatisticsWorking.CurrentAggregationVersion);

                totalDeleted += deleted;
                if (deleted == 0)
                    break;
            }

            logger.LogInformation(
                "Remote retention cleanup completed. RetentionDays={RetentionDays}, CutoffDate={CutoffDate}, AggregationVersion={AggregationVersion}, DeletedRows={DeletedRows}, DurationMs={DurationMs}, ReachedTimeLimit={ReachedTimeLimit}",
                retentionDays,
                cutoffDate,
                RemoteDailyStatisticsWorking.CurrentAggregationVersion,
                totalDeleted,
                stopwatch.ElapsedMilliseconds,
                totalDeleted < maxRows && stopwatch.Elapsed >= maxRunDuration);
        }

        private async Task<int> DeleteBatch(
            int batchSize,
            DateTime cutoffDate,
            int aggregationVersion)
        {
            const string commandText =
                """
                SET NOCOUNT ON;
                SET XACT_ABORT ON;

                CREATE TABLE [#TargetRemoteIds]
                (
                    [Id] bigint NOT NULL PRIMARY KEY
                );

                DECLARE @Remaining int = @BatchSize;

                INSERT INTO [#TargetRemoteIds] ([Id])
                SELECT TOP (@Remaining) [remote].[Id]
                FROM [dbo].[Remotes] AS [remote]
                    WITH (INDEX([IX_Remotes_State_ExecutionTime_Id]), ROWLOCK, READPAST)
                INNER JOIN [dbo].[RemoteDailyAggregationRuns] AS [run]
                    ON [run].[StatisticDate] = CONVERT(date, [remote].[ExecutionTime])
                   AND [run].[AggregationVersion] = @AggregationVersion
                WHERE [remote].[State] = @CompletedState
                  AND [remote].[ExecutionTime] < @CutoffDate
                ORDER BY [remote].[ExecutionTime], [remote].[Id]
                OPTION (LOOP JOIN, FORCE ORDER, MAXDOP 1, RECOMPILE);

                SET @Remaining -= @@ROWCOUNT;

                IF @Remaining > 0
                BEGIN
                    INSERT INTO [#TargetRemoteIds] ([Id])
                    SELECT TOP (@Remaining) [remote].[Id]
                    FROM [dbo].[Remotes] AS [remote]
                        WITH (INDEX([IX_Remotes_State_ExecutionTime_Id]), ROWLOCK, READPAST)
                    INNER JOIN [dbo].[RemoteDailyAggregationRuns] AS [run]
                        ON [run].[StatisticDate] = CONVERT(date, [remote].[ExecutionTime])
                       AND [run].[AggregationVersion] = @AggregationVersion
                    WHERE [remote].[State] = @IncompleteState
                      AND [remote].[ExecutionTime] < @CutoffDate
                    ORDER BY [remote].[ExecutionTime], [remote].[Id]
                    OPTION (LOOP JOIN, FORCE ORDER, MAXDOP 1, RECOMPILE);

                    SET @Remaining -= @@ROWCOUNT;
                END;

                IF @Remaining > 0
                BEGIN
                    INSERT INTO [#TargetRemoteIds] ([Id])
                    SELECT TOP (@Remaining) [remote].[Id]
                    FROM [dbo].[Remotes] AS [remote]
                        WITH (INDEX([IX_Remotes_State_ExecutionTime_Id]), ROWLOCK, READPAST)
                    INNER JOIN [dbo].[RemoteDailyAggregationRuns] AS [run]
                        ON [run].[StatisticDate] = CONVERT(date, [remote].[ExecutionTime])
                       AND [run].[AggregationVersion] = @AggregationVersion
                    WHERE [remote].[State] = @PendingState
                      AND [remote].[ExecutionTime] < @CutoffDate
                      AND NOT EXISTS
                      (
                          SELECT 1
                          FROM [dbo].[UserActivityTags] AS [activity]
                          WHERE [activity].[FK_RemoteId] = [remote].[Id]
                      )
                    ORDER BY [remote].[ExecutionTime], [remote].[Id]
                    OPTION (LOOP JOIN, FORCE ORDER, MAXDOP 1, RECOMPILE);
                END;

                DELETE [remote]
                FROM [dbo].[Remotes] AS [remote] WITH (ROWLOCK, READPAST)
                INNER JOIN [#TargetRemoteIds] AS [target]
                    ON [target].[Id] = [remote].[Id];

                SELECT @@ROWCOUNT;
                """;

            var connection = db.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = commandText;
                command.CommandTimeout = Math.Clamp(
                    options.CommandTimeoutSeconds,
                    30,
                    1_800);

                AddParameter(command, "@BatchSize", batchSize);
                AddParameter(command, "@CutoffDate", cutoffDate.Date);
                AddParameter(command, "@AggregationVersion", aggregationVersion);
                AddParameter(command, "@PendingState", (int)RemoteStateEnum.未處理);
                AddParameter(command, "@CompletedState", (int)RemoteStateEnum.已完成);
                AddParameter(command, "@IncompleteState", (int)RemoteStateEnum.資料不完整);

                var result = await command.ExecuteScalarAsync();
                return result == null || result == DBNull.Value
                    ? 0
                    : Convert.ToInt32(result);
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

        private static void AddParameter(
            IDbCommand command,
            string name,
            object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }
}
