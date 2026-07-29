using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoteDailyStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Popular",
                table: "Prods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RemoteDailyAggregationRuns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatisticDate = table.Column<DateTime>(type: "date", nullable: false),
                    SourceRows = table.Column<long>(type: "bigint", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemoteDailyAggregationRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RemoteDailyStatistics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatisticDate = table.Column<DateTime>(type: "date", nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Scope = table.Column<byte>(type: "tinyint", nullable: false),
                    FK_WebmenuId = table.Column<long>(type: "bigint", nullable: false),
                    FK_ArticleId = table.Column<long>(type: "bigint", nullable: false),
                    FK_ProdId = table.Column<long>(type: "bigint", nullable: false),
                    FK_TechCertId = table.Column<long>(type: "bigint", nullable: false),
                    PageViews = table.Column<long>(type: "bigint", nullable: false),
                    EffectiveViews = table.Column<long>(type: "bigint", nullable: false),
                    LegacyViews = table.Column<long>(type: "bigint", nullable: false),
                    UniqueVisitors = table.Column<long>(type: "bigint", nullable: false),
                    EffectiveUniqueVisitors = table.Column<long>(type: "bigint", nullable: false),
                    TotalVisibleSeconds = table.Column<long>(type: "bigint", nullable: false),
                    AggregatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemoteDailyStatistics", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Popular",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 2L,
                column: "Popular",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 3L,
                column: "Popular",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 4L,
                column: "Popular",
                value: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RemoteDailyAggregationRuns_StatisticDate",
                table: "RemoteDailyAggregationRuns",
                column: "StatisticDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RemoteDailyStatistics_FK_ArticleId_StatisticDate",
                table: "RemoteDailyStatistics",
                columns: new[] { "FK_ArticleId", "StatisticDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RemoteDailyStatistics_FK_ProdId_StatisticDate",
                table: "RemoteDailyStatistics",
                columns: new[] { "FK_ProdId", "StatisticDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RemoteDailyStatistics_FK_WebmenuId_StatisticDate",
                table: "RemoteDailyStatistics",
                columns: new[] { "FK_WebmenuId", "StatisticDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RemoteDailyStatistics_FK_WebsiteId_StatisticDate_Scope",
                table: "RemoteDailyStatistics",
                columns: new[] { "FK_WebsiteId", "StatisticDate", "Scope" });

            migrationBuilder.CreateIndex(
                name: "IX_RemoteDailyStatistics_StatisticDate_FK_WebsiteId_Scope_FK_WebmenuId_FK_ArticleId_FK_ProdId_FK_TechCertId",
                table: "RemoteDailyStatistics",
                columns: new[] { "StatisticDate", "FK_WebsiteId", "Scope", "FK_WebmenuId", "FK_ArticleId", "FK_ProdId", "FK_TechCertId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RemoteDailyAggregationRuns");

            migrationBuilder.DropTable(
                name: "RemoteDailyStatistics");

            migrationBuilder.DropColumn(
                name: "Popular",
                table: "Prods");
        }
    }
}
