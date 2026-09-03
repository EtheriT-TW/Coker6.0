using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddCanvasAuditLogLookupIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CanvasHistory",
                table: "AuditLogs",
                columns: new[] { "FK_WebsiteId", "ServiceName", "MethodName", "ExecutionTime" });

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_FK_WebsiteId",
                table: "AuditLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_FK_WebsiteId",
                table: "AuditLogs",
                column: "FK_WebsiteId");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CanvasHistory",
                table: "AuditLogs");
        }
    }
}
