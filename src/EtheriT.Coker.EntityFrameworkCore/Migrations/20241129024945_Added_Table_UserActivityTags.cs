using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Table_UserActivityTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTagStatistic_Tags_TagId",
                table: "UserTagStatistic");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTagStatistic",
                table: "UserTagStatistic");

            migrationBuilder.RenameTable(
                name: "UserTagStatistic",
                newName: "UserTagStatistics");

            migrationBuilder.RenameIndex(
                name: "IX_UserTagStatistic_TagId",
                table: "UserTagStatistics",
                newName: "IX_UserTagStatistics_TagId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTagStatistics",
                table: "UserTagStatistics",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "UserActivityTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_RemoteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_TId = table.Column<long>(type: "bigint", nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: false),
                    RemoteId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivityTags_Remotes_RemoteId",
                        column: x => x.RemoteId,
                        principalTable: "Remotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityTags_RemoteId",
                table: "UserActivityTags",
                column: "RemoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTagStatistics_Tags_TagId",
                table: "UserTagStatistics",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTagStatistics_Tags_TagId",
                table: "UserTagStatistics");

            migrationBuilder.DropTable(
                name: "UserActivityTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTagStatistics",
                table: "UserTagStatistics");

            migrationBuilder.RenameTable(
                name: "UserTagStatistics",
                newName: "UserTagStatistic");

            migrationBuilder.RenameIndex(
                name: "IX_UserTagStatistics_TagId",
                table: "UserTagStatistic",
                newName: "IX_UserTagStatistic_TagId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTagStatistic",
                table: "UserTagStatistic",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTagStatistic_Tags_TagId",
                table: "UserTagStatistic",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
