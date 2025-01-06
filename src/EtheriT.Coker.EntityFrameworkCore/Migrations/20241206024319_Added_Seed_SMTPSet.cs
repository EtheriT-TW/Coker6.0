using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Seed_SMTPSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "StoreSet",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "StoreSetGroup",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Description", "Image", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[] { 3L, new DateTime(2024, 12, 5, 18, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "", "", null, null, "信件伺服器設定" });

            migrationBuilder.InsertData(
                table: "StoreSetGroup",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Description", "Image", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[] { 4L, new DateTime(2024, 12, 5, 18, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "", "", null, null, "框架設定" });

            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[,]
                {
                    { 9L, new DateTime(2024, 12, 5, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, 3L, null, null, "S001", "SMTPPath", 255, "請輸入SMTP Server", "SMTP Server", "", 1 },
                    { 10L, new DateTime(2024, 12, 5, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, 3L, null, null, "S001", "SMTPPort", 5, "請輸入Port", "Port", "", 8 },
                    { 11L, new DateTime(2024, 12, 5, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, 3L, null, null, "S001", "SMTPAccount", 255, "請輸入 帳號", "帳號", "", 9 },
                    { 12L, new DateTime(2024, 12, 5, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, 3L, null, null, "S001", "SMTPPassword", 50, "請輸入 密碼", "密碼", "", 10 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                table: "StoreSetGroup",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "StoreSetGroup",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "StoreSet",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
