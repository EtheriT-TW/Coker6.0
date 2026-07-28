using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseRetentionIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM [sys].[indexes]
                    WHERE [object_id] = OBJECT_ID(N'[dbo].[Tokens]')
                      AND [name] = N'IX_Tokens_EndTime_id'
                )
                BEGIN
                    CREATE INDEX [IX_Tokens_EndTime_id]
                    ON [dbo].[Tokens] ([EndTime], [id])
                    WHERE [EndTime] IS NOT NULL;
                END;
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM [sys].[indexes]
                    WHERE [object_id] = OBJECT_ID(N'[dbo].[ShoppingCarts]')
                      AND [name] = N'IX_ShoppingCarts_FK_Tid_IsOrder'
                )
                BEGIN
                    CREATE INDEX [IX_ShoppingCarts_FK_Tid_IsOrder]
                    ON [dbo].[ShoppingCarts] ([FK_Tid], [IsOrder]);
                END;
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM [sys].[indexes]
                    WHERE [object_id] = OBJECT_ID(N'[dbo].[AuditLogs]')
                      AND [name] = N'IX_AuditLogs_ExecutionTime'
                )
                BEGIN
                    CREATE INDEX [IX_AuditLogs_ExecutionTime]
                    ON [dbo].[AuditLogs] ([ExecutionTime]);
                END;
                """,
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tokens_EndTime_id",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCarts_FK_Tid_IsOrder",
                table: "ShoppingCarts");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_ExecutionTime",
                table: "AuditLogs");
        }
    }
}
