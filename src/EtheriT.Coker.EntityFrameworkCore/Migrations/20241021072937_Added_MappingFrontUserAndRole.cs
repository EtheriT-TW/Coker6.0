using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_MappingFrontUserAndRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MappingUserAndRoles_FrontUsers_FrontUserId",
                table: "MappingUserAndRoles");

            migrationBuilder.DropIndex(
                name: "IX_MappingUserAndRoles_FrontUserId",
                table: "MappingUserAndRoles");

            migrationBuilder.DropColumn(
                name: "FrontUserId",
                table: "MappingUserAndRoles");

            migrationBuilder.DropColumn(
                name: "IsFront",
                table: "MappingUserAndRoles");

            migrationBuilder.CreateTable(
                name: "MappingFrontUserAndRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    FrontUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingFrontUserAndRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MappingFrontUserAndRoles_FrontUsers_FrontUserId",
                        column: x => x.FrontUserId,
                        principalTable: "FrontUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MappingFrontUserAndRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MappingFrontUserAndRoles_FrontUserId",
                table: "MappingFrontUserAndRoles",
                column: "FrontUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingFrontUserAndRoles_RoleId",
                table: "MappingFrontUserAndRoles",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MappingFrontUserAndRoles");

            migrationBuilder.AddColumn<long>(
                name: "FrontUserId",
                table: "MappingUserAndRoles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFront",
                table: "MappingUserAndRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MappingUserAndRoles_FrontUserId",
                table: "MappingUserAndRoles",
                column: "FrontUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MappingUserAndRoles_FrontUsers_FrontUserId",
                table: "MappingUserAndRoles",
                column: "FrontUserId",
                principalTable: "FrontUsers",
                principalColumn: "Id");
        }
    }
}
