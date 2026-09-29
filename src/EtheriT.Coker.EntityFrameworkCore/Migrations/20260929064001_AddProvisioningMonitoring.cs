using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddProvisioningMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProvisioningAgentStatuses",
                columns: table => new
                {
                    ServerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WorkerId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MachineName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AgentVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DryRun = table.Column<bool>(type: "bit", nullable: false),
                    CpuUsagePercent = table.Column<double>(type: "float", nullable: true),
                    MemoryUsedBytes = table.Column<long>(type: "bigint", nullable: false),
                    MemoryTotalBytes = table.Column<long>(type: "bigint", nullable: false),
                    MemoryUsagePercent = table.Column<double>(type: "float", nullable: true),
                    DiskMetricsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppPoolMetricsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisioningAgentStatuses", x => x.ServerId);
                });

            migrationBuilder.CreateTable(
                name: "ProvisioningMetricSamples",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SampledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CpuUsagePercent = table.Column<double>(type: "float", nullable: true),
                    MemoryUsedBytes = table.Column<long>(type: "bigint", nullable: false),
                    MemoryTotalBytes = table.Column<long>(type: "bigint", nullable: false),
                    MemoryUsagePercent = table.Column<double>(type: "float", nullable: true),
                    DiskMetricsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisioningMetricSamples", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProvisioningAgentStatuses_LastSeenAtUtc",
                table: "ProvisioningAgentStatuses",
                column: "LastSeenAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisioningMetricSamples_SampledAtUtc",
                table: "ProvisioningMetricSamples",
                column: "SampledAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisioningMetricSamples_ServerId_SampledAtUtc",
                table: "ProvisioningMetricSamples",
                columns: new[] { "ServerId", "SampledAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProvisioningAgentStatuses");

            migrationBuilder.DropTable(
                name: "ProvisioningMetricSamples");
        }
    }
}
