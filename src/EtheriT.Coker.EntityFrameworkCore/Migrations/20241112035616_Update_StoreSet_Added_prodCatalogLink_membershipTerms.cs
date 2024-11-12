using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_StoreSet_Added_prodCatalogLink_membershipTerms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "IsDeleted", "LastModificationTime", "LastModifierUserId", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 7L, new DateTime(2024, 11, 12, 11, 59, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, "E001", "prodCatalog", 255, "輸入商品目錄連結，以利前台新增返回目錄按鈕。", "商品目錄", "", 1 });

            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "IsDeleted", "LastModificationTime", "LastModifierUserId", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 8L, new DateTime(2024, 11, 12, 11, 59, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, "E001", "membershipTerms", 5000, "請輸入會員條款內文", "會員條款", "", 2 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 8L);
        }
    }
}
