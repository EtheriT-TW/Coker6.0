using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_StoreSet_Seed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "enable",
                table: "SeoSet");

            migrationBuilder.AddColumn<bool>(
                name: "enable",
                table: "StoreSetDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "pattern",
                table: "SeoSet",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "SeoSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "IsDeleted", "LastModificationTime", "LastModifierUserId", "groupType", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 1L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, false, null, null, 1, "Google", 8, "請輸入GOOGLE提供之驗證碼：xxxxxx-x", "Google Analytics(4)", "\\d6-\\d", 1 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SeoSet",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DropColumn(
                name: "enable",
                table: "StoreSetDetail");

            migrationBuilder.DropColumn(
                name: "pattern",
                table: "SeoSet");

            migrationBuilder.AddColumn<bool>(
                name: "enable",
                table: "SeoSet",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
