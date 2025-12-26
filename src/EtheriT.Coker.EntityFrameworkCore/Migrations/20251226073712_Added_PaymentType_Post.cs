using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Added_PaymentType_Post : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "CanRefund", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "Icons", "LastModificationTime", "LastModifierUserId", "MaxAmount", "MinAmount", "RefundWorkDay", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[] { 29L, false, "Post", new DateTime(2025, 12, 26, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 6L, "", null, null, null, 1, -1, 1, null, "郵政劃撥", false });

            migrationBuilder.InsertData(
                table: "ThirdParties",
                columns: new[] { "Id", "AuditUrl", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "MaxPay", "PaymentUrl", "RefundUrl", "Title", "TokenUrl", "ser_no" },
                values: new object[] { 6L, null, new DateTime(2025, 12, 26, 15, 9, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, null, null, null, null, "郵政劃撥", null, 1 });

            migrationBuilder.InsertData(
                table: "ThirdPartyKeypairs",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_TPid", "LastModificationTime", "LastModifierUserId", "PromptText", "Title" },
                values: new object[,]
                {
                    { 16L, "PostAccount", new DateTime(2025, 12, 26, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 6L, null, null, null, "郵局帳號" },
                    { 17L, "PostName", new DateTime(2025, 12, 26, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 6L, null, null, null, "郵局戶名" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 29L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 16L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 17L);

            migrationBuilder.DeleteData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 6L);
        }
    }
}
