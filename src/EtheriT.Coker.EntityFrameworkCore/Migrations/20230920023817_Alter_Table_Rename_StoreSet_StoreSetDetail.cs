using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_Table_Rename_StoreSet_StoreSetDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreSetDetail_StoreSet_FK_SeoSetId",
                table: "StoreSetDetail");

            migrationBuilder.RenameColumn(
                name: "FK_SeoSetId",
                table: "StoreSetDetail",
                newName: "FK_StoreSetId");

            migrationBuilder.RenameIndex(
                name: "IX_StoreSetDetail_FK_SeoSetId",
                table: "StoreSetDetail",
                newName: "IX_StoreSetDetail_FK_StoreSetId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreSetDetail_StoreSet_FK_StoreSetId",
                table: "StoreSetDetail",
                column: "FK_StoreSetId",
                principalTable: "StoreSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreSetDetail_StoreSet_FK_StoreSetId",
                table: "StoreSetDetail");

            migrationBuilder.RenameColumn(
                name: "FK_StoreSetId",
                table: "StoreSetDetail",
                newName: "FK_SeoSetId");

            migrationBuilder.RenameIndex(
                name: "IX_StoreSetDetail_FK_StoreSetId",
                table: "StoreSetDetail",
                newName: "IX_StoreSetDetail_FK_SeoSetId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreSetDetail_StoreSet_FK_SeoSetId",
                table: "StoreSetDetail",
                column: "FK_SeoSetId",
                principalTable: "StoreSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
