using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoteTrackingCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remotes 是大型高流量資料表。每個 nullable 欄位分開提交，縮短 schema lock，
            // 並以存在檢查確保任一步驟中斷後可以安全重跑。
            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.Remotes', N'AnalyticsAggregatedAt') IS NULL
                    ALTER TABLE [dbo].[Remotes] ADD [AnalyticsAggregatedAt] datetime2 NULL;
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.Remotes', N'EngagedAt') IS NULL
                    ALTER TABLE [dbo].[Remotes] ADD [EngagedAt] datetime2 NULL;
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.Remotes', N'HabitsProcessedAt') IS NULL
                    ALTER TABLE [dbo].[Remotes] ADD [HabitsProcessedAt] datetime2 NULL;
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.Remotes', N'HasInteraction') IS NULL
                    ALTER TABLE [dbo].[Remotes] ADD [HasInteraction] bit NULL;
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.Remotes', N'IsEngaged') IS NULL
                    ALTER TABLE [dbo].[Remotes] ADD [IsEngaged] bit NULL;
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.Remotes', N'LastHeartbeatAt') IS NULL
                    ALTER TABLE [dbo].[Remotes] ADD [LastHeartbeatAt] datetime2 NULL;
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.Remotes', N'TrackingEventId') IS NULL
                    ALTER TABLE [dbo].[Remotes] ADD [TrackingEventId] uniqueidentifier NULL;
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.Remotes', N'TrafficQuality') IS NULL
                    ALTER TABLE [dbo].[Remotes] ADD [TrafficQuality] int NULL;
                """,
                suppressTransaction: true);

            // SQL Server 2019 Standard 不支援 online index create；必要的 filtered index
            // 獨立建立，避免失敗時回滾已完成的欄位異動。
            migrationBuilder.Sql(
                """
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM [sys].[indexes]
                    WHERE [object_id] = OBJECT_ID(N'[dbo].[Remotes]')
                      AND [name] = N'IX_Remotes_TrackingEventId'
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_Remotes_TrackingEventId]
                        ON [dbo].[Remotes] ([TrackingEventId])
                        WHERE [TrackingEventId] IS NOT NULL;
                END
                """,
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Remotes_TrackingEventId",
                table: "Remotes");

            migrationBuilder.DropColumn(
                name: "AnalyticsAggregatedAt",
                table: "Remotes");

            migrationBuilder.DropColumn(
                name: "EngagedAt",
                table: "Remotes");

            migrationBuilder.DropColumn(
                name: "HabitsProcessedAt",
                table: "Remotes");

            migrationBuilder.DropColumn(
                name: "HasInteraction",
                table: "Remotes");

            migrationBuilder.DropColumn(
                name: "IsEngaged",
                table: "Remotes");

            migrationBuilder.DropColumn(
                name: "LastHeartbeatAt",
                table: "Remotes");

            migrationBuilder.DropColumn(
                name: "TrackingEventId",
                table: "Remotes");

            migrationBuilder.DropColumn(
                name: "TrafficQuality",
                table: "Remotes");
        }
    }
}
