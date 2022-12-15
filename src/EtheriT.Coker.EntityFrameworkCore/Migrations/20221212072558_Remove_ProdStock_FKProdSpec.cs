using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Remove_ProdStock_FKProdSpec : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Stocks_Prod_Specs_FK_S2id",
                table: "Prod_Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Prod_Stocks_FK_S2id",
                table: "Prod_Stocks");

            migrationBuilder.UpdateData(
                table: "Prod_Spec_Types",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459));

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "FK_S1id", "FK_S2id", "Min_Qty", "Safe_Qty" },
                values: new object[] { 0L, 0L, 1, 5 });

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "FK_S1id", "FK_S2id", "Min_Qty", "Safe_Qty" },
                values: new object[] { 0L, 0L, 1, 5 });

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "FK_S1id", "FK_S2id", "Min_Qty", "Safe_Qty" },
                values: new object[] { 0L, 0L, 1, 5 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Prod_Spec_Types",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1460));

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "FK_S1id", "FK_S2id", "Min_Qty", "Safe_Qty" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "FK_S1id", "FK_S2id", "Min_Qty", "Safe_Qty" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "FK_S1id", "FK_S2id", "Min_Qty", "Safe_Qty" },
                values: new object[] { null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Stocks_FK_S2id",
                table: "Prod_Stocks",
                column: "FK_S2id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_Stocks_Prod_Specs_FK_S2id",
                table: "Prod_Stocks",
                column: "FK_S2id",
                principalTable: "Prod_Specs",
                principalColumn: "Id");
        }
    }
}
