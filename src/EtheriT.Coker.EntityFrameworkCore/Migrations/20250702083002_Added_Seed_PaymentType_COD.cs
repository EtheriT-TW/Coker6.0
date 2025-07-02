using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Added_Seed_PaymentType_COD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "CanRefund", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "Icons", "LastModificationTime", "LastModifierUserId", "MaxAmount", "MinAmount", "RefundWorkDay", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[] { 28L, false, "COD", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 5L, "pay05.jpg", null, null, null, 1, -1, 1, null, "貨到付款", false });

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 1L,
                column: "ser_no",
                value: 1);

            migrationBuilder.InsertData(
                table: "ThirdParties",
                columns: new[] { "Id", "AuditUrl", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "MaxPay", "PaymentUrl", "RefundUrl", "Title", "TokenUrl", "ser_no" },
                values: new object[] { 5L, null, new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, null, null, null, null, "貨到付款", null, 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 28L);

            migrationBuilder.DeleteData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 1L,
                column: "ser_no",
                value: 500);
        }
    }
}
