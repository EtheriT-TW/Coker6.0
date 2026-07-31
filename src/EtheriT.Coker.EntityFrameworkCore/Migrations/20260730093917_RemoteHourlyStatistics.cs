using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoteHourlyStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RemoteHourlyStatistics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatisticHour = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_RemoteHourlyStatistics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RemoteHourlyStatistics_FK_WebsiteId_StatisticHour",
                table: "RemoteHourlyStatistics",
                columns: new[] { "FK_WebsiteId", "StatisticHour" });

            migrationBuilder.CreateIndex(
                name: "IX_RemoteHourlyStatistics_StatisticHour_FK_WebsiteId",
                table: "RemoteHourlyStatistics",
                columns: new[] { "StatisticHour", "FK_WebsiteId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RemoteHourlyStatistics");
        }
    }
}
