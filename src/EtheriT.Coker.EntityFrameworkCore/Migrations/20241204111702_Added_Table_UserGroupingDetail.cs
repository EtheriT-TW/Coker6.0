using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Table_UserGroupingDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserGroupingDetails",
                columns: table => new
                {
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_GropingId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupingDetails", x => new { x.UUID, x.FK_GropingId });
                    table.ForeignKey(
                        name: "FK_UserGroupingDetails_UserGroupings_FK_GropingId",
                        column: x => x.FK_GropingId,
                        principalTable: "UserGroupings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupingDetails_FK_GropingId",
                table: "UserGroupingDetails",
                column: "FK_GropingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGroupingDetails");
        }
    }
}
