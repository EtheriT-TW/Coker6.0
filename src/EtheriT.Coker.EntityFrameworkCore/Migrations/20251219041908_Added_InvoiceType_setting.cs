using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Added_InvoiceType_setting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Carrier",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceType",
                table: "Order_Headers",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PersonalInvoiceType",
                table: "Order_Headers",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "Level", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[,]
                {
                    { 24L, new DateTime(2025, 12, 16, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, 2L, null, null, null, "M001", "HasInvoice", null, "訂單是否供客戶選擇開立發票方式。", "開立發票", "", 5 },
                    { 25L, new DateTime(2025, 12, 16, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, 2L, null, null, null, "M001", "ExtraInviiceCarrier", null, "允許用戶使用發票載具類型。", "發票載具", "", 4 }
                });

            migrationBuilder.InsertData(
                table: "StoreSetItems",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetId", "IsDefault", "Key", "LastModificationTime", "LastModifierUserId", "Level", "Value" },
                values: new object[,]
                {
                    { 15L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 24L, false, "EnabledInvoice", null, null, null, "允許" },
                    { 16L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 24L, false, "DisabledInvoice", null, null, null, "不允許" },
                    { 17L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 25L, false, "MobileCarrier", null, null, null, "手機載具" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FrontUsers_FK_User",
                table: "FrontUsers",
                column: "FK_User");

            migrationBuilder.AddForeignKey(
                name: "FK_FrontUsers_Users_FK_User",
                table: "FrontUsers",
                column: "FK_User",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FrontUsers_Users_FK_User",
                table: "FrontUsers");

            migrationBuilder.DropIndex(
                name: "IX_FrontUsers_FK_User",
                table: "FrontUsers");

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 15L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 16L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 17L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 24L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 25L);

            migrationBuilder.DropColumn(
                name: "Carrier",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "InvoiceType",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "PersonalInvoiceType",
                table: "Order_Headers");
        }
    }
}
