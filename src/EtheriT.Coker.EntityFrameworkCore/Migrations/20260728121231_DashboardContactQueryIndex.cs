using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class DashboardContactQueryIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Contacts_FK_WebMenuId_Status_CreationTime_Active",
                table: "Contacts",
                columns: new[] { "FK_WebMenuId", "Status", "CreationTime" },
                filter: "[IsDeleted] = 0")
                .Annotation("SqlServer:Include", new[] { "Name", "UserName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contacts_FK_WebMenuId_Status_CreationTime_Active",
                table: "Contacts");
        }
    }
}
