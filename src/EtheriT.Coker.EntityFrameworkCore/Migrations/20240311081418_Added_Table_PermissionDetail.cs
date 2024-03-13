using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Table_PermissionDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PermissionDetail",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_UserId = table.Column<long>(type: "bigint", nullable: true),
                    FK_RoleId = table.Column<long>(type: "bigint", nullable: true),
                    FK_TargetId = table.Column<long>(type: "bigint", nullable: true),
                    IsGranted = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionDetail_Roles_FK_RoleId",
                        column: x => x.FK_RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PermissionDetail_Users_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PermissionDetail_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDetail_FK_RoleId",
                table: "PermissionDetail",
                column: "FK_RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDetail_FK_UserId",
                table: "PermissionDetail",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDetail_FK_WebsiteId",
                table: "PermissionDetail",
                column: "FK_WebsiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionDetail");
        }
    }
}
