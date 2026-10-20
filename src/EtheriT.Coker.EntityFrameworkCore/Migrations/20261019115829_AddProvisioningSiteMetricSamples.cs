using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddProvisioningSiteMetricSamples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProvisioningSiteMetricSamples",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApplicationPoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SampledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrivateMemoryBytes = table.Column<long>(type: "bigint", nullable: false),
                    WorkingSetBytes = table.Column<long>(type: "bigint", nullable: false),
                    CpuUsagePercent = table.Column<double>(type: "float", nullable: true),
                    OnlineVisitors = table.Column<long>(type: "bigint", nullable: true),
                    WebsiteIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebsiteNamesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisioningSiteMetricSamples", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProvisioningSiteMetricSamples_SampledAtUtc",
                table: "ProvisioningSiteMetricSamples",
                column: "SampledAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ProvisioningSiteMetricSamples_ServerId_ApplicationPoolName_SampledAtUtc",
                table: "ProvisioningSiteMetricSamples",
                columns: new[] { "ServerId", "ApplicationPoolName", "SampledAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProvisioningSiteMetricSamples");
        }
    }
}
