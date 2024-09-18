using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Seed_StoreSet_storeMemo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "IsDeleted", "LastModificationTime", "LastModifierUserId", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 6L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, "E001", "linkMore", 255, "輸入一段連結，在商品頁中可以顯示了解更多按鈕。", "了解更多", "", 1 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 6L);
        }
    }
}
