using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Seed_StoreSet_GoogleAds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 13L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 1L, null, null, "S001", "GoogleAds", 14, "需先埋入GA，再輸入GOOGLE提供之轉換 ID：AW-xxxxxxxxxxx", "Google Ads", "^AW-\\w+", 1 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 13L);
        }
    }
}
