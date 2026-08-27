using System.Data;
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

        [AutomaticRetry(Attempts = 1)]
        [DisableConcurrentExecution(3600)]
        public async Task CleanupAggregatedRemotes()
        {
            var retentionDays = Math.Max(1, options.RemoteRetentionDays);
            var batchSize = Math.Clamp(options.RemoteBatchSize, 100, 5_000);
            var maxRows = Math.Max(batchSize, options.RemoteMaxRowsPerRun);
            var cutoffDate = DateTime.Today.AddDays(-retentionDays);
            var totalDeleted = 0;

            while (totalDeleted < maxRows)
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
                "Remote retention cleanup completed. RetentionDays={RetentionDays}, CutoffDate={CutoffDate}, AggregationVersion={AggregationVersion}, DeletedRows={DeletedRows}",
                retentionDays,
                cutoffDate,
                RemoteDailyStatisticsWorking.CurrentAggregationVersion,
                totalDeleted);
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

                DECLARE @TargetDate date;

                SELECT TOP (1)
                    @TargetDate = [run].[StatisticDate]
                FROM [dbo].[RemoteDailyAggregationRuns] AS [run]
                WHERE [run].[AggregationVersion] = @AggregationVersion
                  AND [run].[StatisticDate] < @CutoffDate
                  AND EXISTS
                  (
                      SELECT 1
                      FROM [dbo].[Remotes] AS [remote] WITH (READPAST)
                      WHERE [remote].[ExecutionTime] >= [run].[StatisticDate]
                        AND [remote].[ExecutionTime] < DATEADD(day, 1, [run].[StatisticDate])
                        AND
                        (
                            [remote].[State] <> @PendingState
                            OR NOT EXISTS
                            (
                                SELECT 1
                                FROM [dbo].[UserActivityTags] AS [activity]
                                WHERE [activity].[FK_RemoteId] = [remote].[Id]
                            )
                        )
                  )
                ORDER BY [run].[StatisticDate];

                IF @TargetDate IS NULL
                BEGIN
                    SELECT 0;
                    RETURN;
                END;

                DELETE TOP (@BatchSize) [remote]
                FROM [dbo].[Remotes] AS [remote] WITH (ROWLOCK, READPAST)
                WHERE [remote].[ExecutionTime] >= @TargetDate
                  AND [remote].[ExecutionTime] < DATEADD(day, 1, @TargetDate)
                  AND
                  (
                      [remote].[State] <> @PendingState
                      OR NOT EXISTS
                      (
                          SELECT 1
                          FROM [dbo].[UserActivityTags] AS [activity]
                          WHERE [activity].[FK_RemoteId] = [remote].[Id]
                      )
                  );

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
