using System.Data;
using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Application.BackgroundJob
{
    public sealed class DatabaseRetentionWorking
    {
        private readonly CokerDbContext db;
        private readonly DatabaseRetentionOptions options;
        private readonly ILogger<DatabaseRetentionWorking> logger;

        public DatabaseRetentionWorking(
            CokerDbContext db,
            IOptions<DatabaseRetentionOptions> options,
            ILogger<DatabaseRetentionWorking> logger)
        {
            this.db = db;
            this.options = options.Value;
            this.logger = logger;
        }

        [AutomaticRetry(Attempts = 1)]
        [DisableConcurrentExecution(3600)]
        public async Task CleanupAuditLogs()
        {
            var retentionDays = Math.Max(1, options.AuditLogRetentionDays);
            var batchSize = Math.Clamp(options.AuditLogBatchSize, 1, 2_000);
            var maxRows = Math.Max(batchSize, options.AuditLogMaxRowsPerRun);
            var cutoff = DateTime.Now.AddDays(-retentionDays);
            var totalDeleted = 0;

            while (totalDeleted < maxRows)
            {
                var currentBatchSize = Math.Min(batchSize, maxRows - totalDeleted);
                var deleted = await ExecuteScalarAsync(
                    """
                    SET NOCOUNT ON;

                    DELETE TOP (@BatchSize)
                    FROM [dbo].[AuditLogs] WITH (ROWLOCK, READPAST)
                    WHERE [ExecutionTime] < @Cutoff;

                    SELECT @@ROWCOUNT;
                    """,
                    ("@BatchSize", currentBatchSize),
                    ("@Cutoff", cutoff));

                totalDeleted += deleted;
                if (deleted < currentBatchSize)
                    break;
            }

            logger.LogInformation(
                "AuditLogs retention cleanup completed. RetentionDays={RetentionDays}, Cutoff={Cutoff}, DeletedRows={DeletedRows}",
                retentionDays,
                cutoff,
                totalDeleted);
        }

        [AutomaticRetry(Attempts = 1)]
        [DisableConcurrentExecution(3600)]
        public async Task CleanupExpiredTokens()
        {
            var batchSize = Math.Clamp(options.TokenBatchSize, 1, 5_000);
            var maxRows = Math.Max(batchSize, options.TokenMaxRowsPerRun);
            var totalDeleted = 0;

            while (totalDeleted < maxRows)
            {
                var currentBatchSize = Math.Min(batchSize, maxRows - totalDeleted);
                var deleted = await ExecuteScalarAsync(
                    """
                    SET NOCOUNT ON;
                    SET XACT_ABORT ON;

                    DECLARE @ExpiredTokens TABLE
                    (
                        [Id] uniqueidentifier NOT NULL PRIMARY KEY
                    );

                    DECLARE @CartsToDelete TABLE
                    (
                        [Id] bigint NOT NULL PRIMARY KEY
                    );

                    BEGIN TRANSACTION;

                    INSERT INTO @ExpiredTokens ([Id])
                    SELECT TOP (@BatchSize) [id]
                    FROM [dbo].[Tokens] WITH (UPDLOCK, READPAST, ROWLOCK)
                    WHERE [EndTime] IS NOT NULL
                      AND [EndTime] < @Now
                    ORDER BY [EndTime], [id];

                    DECLARE @TokenCount int = @@ROWCOUNT;

                    INSERT INTO @CartsToDelete ([Id])
                    SELECT DISTINCT [cart].[Id]
                    FROM [dbo].[ShoppingCarts] AS [cart] WITH (UPDLOCK, READPAST, ROWLOCK)
                    INNER JOIN @ExpiredTokens AS [expired]
                        ON [cart].[FK_Tid] = [expired].[Id]
                    WHERE [cart].[IsOrder] = CAST(0 AS bit)
                      AND NOT EXISTS
                      (
                          SELECT 1
                          FROM [dbo].[FrontUsers] AS [member]
                          WHERE [member].[UUID] = [cart].[UUID]
                             OR
                             (
                                 [cart].[FK_Uid] IS NOT NULL
                                 AND [cart].[FK_Uid] > 0
                                 AND [member].[FK_User] = [cart].[FK_Uid]
                             )
                      )
                      AND NOT EXISTS
                      (
                          SELECT 1
                          FROM [dbo].[MappingOldNewUUID] AS [mapping]
                          INNER JOIN [dbo].[FrontUsers] AS [member]
                              ON [member].[UUID] = [mapping].[UserUUID]
                          WHERE [mapping].[TempUUID] = [cart].[UUID]
                      );

                    DELETE [map]
                    FROM [dbo].[TokenMapShoppingCarts] AS [map]
                    INNER JOIN @ExpiredTokens AS [expired]
                        ON [map].[UUID] = [expired].[Id];

                    DELETE [map]
                    FROM [dbo].[TokenMapShoppingCarts] AS [map]
                    INNER JOIN @CartsToDelete AS [cart]
                        ON [map].[FK_Tid] = [cart].[Id];

                    DELETE [cart]
                    FROM [dbo].[ShoppingCarts] AS [cart]
                    INNER JOIN @CartsToDelete AS [target]
                        ON [cart].[Id] = [target].[Id];

                    DELETE [token]
                    FROM [dbo].[Tokens] AS [token]
                    INNER JOIN @ExpiredTokens AS [expired]
                        ON [token].[id] = [expired].[Id];

                    COMMIT TRANSACTION;

                    SELECT @TokenCount;
                    """,
                    ("@BatchSize", currentBatchSize),
                    ("@Now", DateTime.Now));

                totalDeleted += deleted;
                if (deleted < currentBatchSize)
                    break;
            }

            var orphanCartMaxRows = Math.Max(batchSize, options.OrphanCartMaxRowsPerRun);
            var totalDeletedOrphanCarts = 0;

            while (totalDeletedOrphanCarts < orphanCartMaxRows)
            {
                var currentBatchSize = Math.Min(
                    batchSize,
                    orphanCartMaxRows - totalDeletedOrphanCarts);
                var deleted = await ExecuteScalarAsync(
                    """
                    SET NOCOUNT ON;
                    SET XACT_ABORT ON;

                    DECLARE @OrphanCarts TABLE
                    (
                        [Id] bigint NOT NULL PRIMARY KEY
                    );

                    BEGIN TRANSACTION;

                    INSERT INTO @OrphanCarts ([Id])
                    SELECT TOP (@BatchSize) [cart].[Id]
                    FROM [dbo].[ShoppingCarts] AS [cart] WITH (UPDLOCK, READPAST, ROWLOCK)
                    LEFT JOIN [dbo].[Tokens] AS [token]
                        ON [token].[id] = [cart].[FK_Tid]
                    WHERE [cart].[IsOrder] = CAST(0 AS bit)
                      AND [token].[id] IS NULL
                      AND NOT EXISTS
                      (
                          SELECT 1
                          FROM [dbo].[FrontUsers] AS [member]
                          WHERE [member].[UUID] = [cart].[UUID]
                             OR
                             (
                                 [cart].[FK_Uid] IS NOT NULL
                                 AND [cart].[FK_Uid] > 0
                                 AND [member].[FK_User] = [cart].[FK_Uid]
                             )
                      )
                      AND NOT EXISTS
                      (
                          SELECT 1
                          FROM [dbo].[MappingOldNewUUID] AS [mapping]
                          INNER JOIN [dbo].[FrontUsers] AS [member]
                              ON [member].[UUID] = [mapping].[UserUUID]
                          WHERE [mapping].[TempUUID] = [cart].[UUID]
                      )
                    ORDER BY [cart].[Id];

                    DECLARE @CartCount int = @@ROWCOUNT;

                    DELETE [map]
                    FROM [dbo].[TokenMapShoppingCarts] AS [map]
                    INNER JOIN @OrphanCarts AS [cart]
                        ON [map].[FK_Tid] = [cart].[Id];

                    DELETE [cart]
                    FROM [dbo].[ShoppingCarts] AS [cart]
                    INNER JOIN @OrphanCarts AS [target]
                        ON [cart].[Id] = [target].[Id];

                    COMMIT TRANSACTION;

                    SELECT @CartCount;
                    """,
                    ("@BatchSize", currentBatchSize));

                totalDeletedOrphanCarts += deleted;
                if (deleted < currentBatchSize)
                    break;
            }

            logger.LogInformation(
                "Expired token cleanup completed. DeletedTokens={DeletedTokens}, DeletedOrphanCarts={DeletedOrphanCarts}",
                totalDeleted,
                totalDeletedOrphanCarts);
        }

        private async Task<int> ExecuteScalarAsync(
            string commandText,
            params (string Name, object Value)[] parameters)
        {
            var connection = db.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = commandText;
                command.CommandTimeout = Math.Clamp(options.CommandTimeoutSeconds, 30, 1_800);

                foreach (var (name, value) in parameters)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = name;
                    parameter.Value = value;
                    command.Parameters.Add(parameter);
                }

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
    }
}
