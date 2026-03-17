using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Update_ThirdParty_Add_ServiceType_ThirdPartyKeypair_Add_InputType_Seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InputType",
                table: "ThirdPartyKeypairs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ServiceType",
                table: "ThirdParties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 1L,
                column: "ServiceType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 2L,
                column: "ServiceType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 3L,
                column: "ServiceType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 4L,
                column: "ServiceType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 5L,
                column: "ServiceType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 6L,
                column: "ServiceType",
                value: 1);

            migrationBuilder.InsertData(
                table: "ThirdParties",
                columns: new[] { "Id", "AuditUrl", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "MaxPay", "PaymentUrl", "RefundUrl", "ServiceType", "Title", "TokenUrl", "ser_no" },
                values: new object[] { 7L, null, new DateTime(2025, 12, 26, 15, 9, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, null, null, null, null, 2, "綠界物流", null, 1 });

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 1L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 2L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 3L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 4L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 5L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 6L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 7L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 8L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 9L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 10L,
                columns: new[] { "Code", "InputType" },
                values: new object[] { "PlatformID", 1 });

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 11L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 12L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 13L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 14L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 15L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 16L,
                column: "InputType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 17L,
                column: "InputType",
                value: 1);

            migrationBuilder.InsertData(
                table: "ThirdPartyKeypairs",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_TPid", "InputType", "LastModificationTime", "LastModifierUserId", "PromptText", "Title" },
                values: new object[,]
                {
                    { 18L, "MerchantID", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 1, null, null, null, "商店代號" },
                    { 19L, "PlatformID", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 1, null, null, "※非專案合作請留空", "平台代號" },
                    { 20L, "HashKey", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 1, null, null, null, "HashKey" },
                    { 21L, "HashIV", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 1, null, null, null, "HashIV" },
                    { 22L, "EnableB2C", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 2, null, null, null, "是否啟用大宗寄倉" },
                    { 23L, "EnableC2C", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 2, null, null, null, "是否啟用超商門市寄/取件" },
                    { 24L, "IsCollection", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 2, null, null, null, "是否代收貨款" },
                    { 25L, "EnableHomeDelivery", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 2, null, null, null, "是否啟用宅配" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 18L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 19L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 20L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 21L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 22L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 23L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 24L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 25L);

            migrationBuilder.DeleteData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DropColumn(
                name: "InputType",
                table: "ThirdPartyKeypairs");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "ThirdParties");

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 10L,
                column: "Code",
                value: "PlatformID ");
        }
    }
}
