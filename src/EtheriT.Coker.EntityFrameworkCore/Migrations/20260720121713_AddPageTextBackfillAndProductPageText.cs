using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddPageTextBackfillAndProductPageText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PageText",
                table: "Prods",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PageTextBackfillStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TargetMaxId = table.Column<long>(type: "bigint", nullable: false),
                    LastProcessedId = table.Column<long>(type: "bigint", nullable: false),
                    TotalCount = table.Column<int>(type: "int", nullable: false),
                    ProcessedCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    RemainingNullCount = table.Column<int>(type: "int", nullable: false),
                    FailedIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageTextBackfillStates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageTextBackfillStates_FK_WebsiteId_ContentType",
                table: "PageTextBackfillStates",
                columns: new[] { "FK_WebsiteId", "ContentType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageTextBackfillStates_Status_LastModificationTime",
                table: "PageTextBackfillStates",
                columns: new[] { "Status", "LastModificationTime" });

            // Create the indexes now with automatic change tracking. PageText is
            // filled later by the resumable Hangfire job; search is switched only
            // after the maintenance status reports that all three sources are ready.
            migrationBuilder.Sql("""
                IF FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 1
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE [name] = N'CokerSearchCatalog')
                        CREATE FULLTEXT CATALOG [CokerSearchCatalog] AS DEFAULT;

                    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'[dbo].[WebMenus]'))
                        CREATE FULLTEXT INDEX ON [dbo].[WebMenus]
                        (
                            [Title] LANGUAGE 1028,
                            [Description] LANGUAGE 1028,
                            [PageText] LANGUAGE 1028
                        ) KEY INDEX [PK_WebMenus] ON [CokerSearchCatalog] WITH CHANGE_TRACKING AUTO;

                    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'[dbo].[Article]'))
                        CREATE FULLTEXT INDEX ON [dbo].[Article]
                        (
                            [Title] LANGUAGE 1028,
                            [Description] LANGUAGE 1028,
                            [PageText] LANGUAGE 1028
                        ) KEY INDEX [PK_Article] ON [CokerSearchCatalog] WITH CHANGE_TRACKING AUTO;

                    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'[dbo].[Prods]'))
                        CREATE FULLTEXT INDEX ON [dbo].[Prods]
                        (
                            [Title] LANGUAGE 1028,
                            [ItemNo] LANGUAGE 0,
                            [Introduction] LANGUAGE 1028,
                            [Description] LANGUAGE 1028,
                            [PageText] LANGUAGE 1028
                        ) KEY INDEX [PK_Prods] ON [CokerSearchCatalog] WITH CHANGE_TRACKING AUTO;
                END
                """, suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'[dbo].[Prods]'))
                    DROP FULLTEXT INDEX ON [dbo].[Prods];
                IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'[dbo].[Article]'))
                    DROP FULLTEXT INDEX ON [dbo].[Article];
                IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'[dbo].[WebMenus]'))
                    DROP FULLTEXT INDEX ON [dbo].[WebMenus];
                """, suppressTransaction: true);

            migrationBuilder.DropTable(
                name: "PageTextBackfillStates");

            migrationBuilder.DropColumn(
                name: "PageText",
                table: "Prods");
        }
    }
}
