using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoteOnlineVisitorIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Remotes_FK_WebsiteId_LastHeartbeatAt_Online",
                table: "Remotes",
                columns: new[] { "FK_WebsiteId", "LastHeartbeatAt" },
                filter: "[LastHeartbeatAt] IS NOT NULL AND [TrackingEventId] IS NOT NULL")
                .Annotation("SqlServer:Include", new[] { "TrackingEventId", "FK_UserId", "UUID", "IsEngaged" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Remotes_FK_WebsiteId_LastHeartbeatAt_Online",
                table: "Remotes");
        }
    }
}
