using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Add_ProdSpec_ProdSpecType_Prod_ProdStock_Seed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "permanent",
                table: "Prods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Prod_Spec_Types",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_WebsiteId", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Type" },
                values: new object[,]
                {
                    { 1L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 2L, false, null, null, "color" },
                    { 2L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1460), 2L, null, null, 2L, false, null, null, "size" }
                });

            migrationBuilder.InsertData(
                table: "Prods",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Description", "Discount", "Disp_Opt", "EndTime", "FK_WebsiteId", "Introduction", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Price", "Ser_No", "StartTime", "Title", "permanent" },
                values: new object[,]
                {
                    { 1L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, "商品一的說明", null, true, null, 2L, "商品一的介紹", false, null, null, 28000.0, 500, null, "商品一的名稱", false },
                    { 2L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, "商品二的說明", null, true, null, 2L, "商品二的介紹", false, null, null, 9500.0, 500, null, "商品二的名稱", false },
                    { 3L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, "商品三的說明", null, true, null, 2L, "商品三的介紹", false, null, null, 13000.0, 500, null, "商品三的名稱", false }
                });

            migrationBuilder.InsertData(
                table: "Prod_Specs",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_Tid", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[,]
                {
                    { 1L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1461), 2L, null, null, 1L, false, null, null, "white" },
                    { 2L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1462), 2L, null, null, 1L, false, null, null, "gray" },
                    { 3L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1463), 2L, null, null, 1L, false, null, null, "black" },
                    { 4L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1464), 2L, null, null, 2L, false, null, null, "small" },
                    { 5L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1465), 2L, null, null, 2L, false, null, null, "medium" },
                    { 6L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1466), 2L, null, null, 2L, false, null, null, "large" }
                });

            migrationBuilder.InsertData(
                table: "Prod_Stocks",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_Pid", "FK_S1id", "FK_S2id", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Safe_Qty", "Ser_No", "Stock" },
                values: new object[,]
                {
                    { 1L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 1L, null, null, false, null, null, null, 500, 100 },
                    { 2L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 2L, null, null, false, null, null, null, 500, 100 },
                    { 3L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 3L, null, null, false, null, null, null, 500, 100 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Prod_Spec_Types",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Prod_Spec_Types",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DropColumn(
                name: "permanent",
                table: "Prods");
        }
    }
}
