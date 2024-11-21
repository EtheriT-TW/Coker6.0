using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Seed_PChomePay_AddIBRCD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 9L,
                column: "IsDeleted",
                value: true);

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "IsDeleted", "LastModificationTime", "LastModifierUserId", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[] { 15L, "PCHomeIBRCD", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, 500, null, "超商條碼付款", false });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 15L);

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 9L,
                column: "IsDeleted",
                value: false);
        }
    }
}
