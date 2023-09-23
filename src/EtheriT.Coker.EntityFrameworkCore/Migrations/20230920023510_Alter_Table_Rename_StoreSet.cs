using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_Table_Rename_StoreSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreSetDetail_SeoSet_FK_SeoSetId",
                table: "StoreSetDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SeoSet",
                table: "SeoSet");

            migrationBuilder.RenameTable(
                name: "SeoSet",
                newName: "StoreSet");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StoreSet",
                table: "StoreSet",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreSetDetail_StoreSet_FK_SeoSetId",
                table: "StoreSetDetail",
                column: "FK_SeoSetId",
                principalTable: "StoreSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreSetDetail_StoreSet_FK_SeoSetId",
                table: "StoreSetDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StoreSet",
                table: "StoreSet");

            migrationBuilder.RenameTable(
                name: "StoreSet",
                newName: "SeoSet");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SeoSet",
                table: "SeoSet",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreSetDetail_SeoSet_FK_SeoSetId",
                table: "StoreSetDetail",
                column: "FK_SeoSetId",
                principalTable: "SeoSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
