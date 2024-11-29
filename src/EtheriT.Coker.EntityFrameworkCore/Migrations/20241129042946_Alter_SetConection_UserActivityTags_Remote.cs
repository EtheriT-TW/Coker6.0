using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_SetConection_UserActivityTags_Remote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserActivityTags_Remotes_RemoteId",
                table: "UserActivityTags");

            migrationBuilder.DropIndex(
                name: "IX_UserActivityTags_RemoteId",
                table: "UserActivityTags");

            migrationBuilder.DropColumn(
                name: "RemoteId",
                table: "UserActivityTags");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityTags_FK_RemoteId",
                table: "UserActivityTags",
                column: "FK_RemoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivityTags_Remotes_FK_RemoteId",
                table: "UserActivityTags",
                column: "FK_RemoteId",
                principalTable: "Remotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserActivityTags_Remotes_FK_RemoteId",
                table: "UserActivityTags");

            migrationBuilder.DropIndex(
                name: "IX_UserActivityTags_FK_RemoteId",
                table: "UserActivityTags");

            migrationBuilder.AddColumn<long>(
                name: "RemoteId",
                table: "UserActivityTags",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityTags_RemoteId",
                table: "UserActivityTags",
                column: "RemoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivityTags_Remotes_RemoteId",
                table: "UserActivityTags",
                column: "RemoteId",
                principalTable: "Remotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
