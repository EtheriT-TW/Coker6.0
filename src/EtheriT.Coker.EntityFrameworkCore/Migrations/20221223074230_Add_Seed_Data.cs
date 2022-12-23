using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Add_Seed_Data : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Prod_Spec_Types",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_WebsiteId", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Type" },
                values: new object[] { 3L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 2L, false, null, null, "其他" });

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "FK_Pid", "FK_S1id", "FK_S2id", "Price" },
                values: new object[] { 1L, 2L, 5L, 28500.0 });

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "FK_Pid", "Price" },
                values: new object[] { 2L, 9500.0 });

            migrationBuilder.InsertData(
                table: "Prod_Stocks",
                columns: new[] { "Id", "Alert_Qty", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_Pid", "FK_S1id", "FK_S2id", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Min_Qty", "Price", "Ser_No", "Stock" },
                values: new object[] { 5L, 5, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 3L, 1L, 4L, false, null, null, 1, 13000.0, 500, 100 });

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 1L,
                column: "permanent",
                value: true);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 2L,
                column: "permanent",
                value: true);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 3L,
                column: "permanent",
                value: true);

            migrationBuilder.InsertData(
                table: "Prods",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Description", "Discount", "Disp_Opt", "EndTime", "FK_WebsiteId", "Introduction", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Ser_No", "StartTime", "Title", "permanent" },
                values: new object[] { 4L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, "L183NA檯上奈米方型盆W560 x D380 x H120mm\n1033PH四角型單孔單槍加高面盆龍頭歐洲省水二段Ø35短腳陶瓷心軸(附歐規按押無溢水排桿)", null, true, null, 2L, "最大容水量：11公升\n適用水壓：1~5kgf/㎝²", false, null, null, 500, null, "L183NA 檯上奈米方型盆", true });

            migrationBuilder.InsertData(
                table: "Prod_Specs",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_Tid", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[,]
                {
                    { 7L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1466), 2L, null, null, 3L, false, null, null, "整組" },
                    { 8L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1466), 2L, null, null, 3L, false, null, null, "L183NA 檯上奈米方型盆" },
                    { 9L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1466), 2L, null, null, 3L, false, null, null, "1033PH 四角型單孔單槍加高面盆龍頭" }
                });

            migrationBuilder.InsertData(
                table: "Prod_Stocks",
                columns: new[] { "Id", "Alert_Qty", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_Pid", "FK_S1id", "FK_S2id", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Min_Qty", "Price", "Ser_No", "Stock" },
                values: new object[,]
                {
                    { 6L, 5, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 4L, 7L, 0L, false, null, null, 1, 24300.0, 500, 100 },
                    { 7L, 5, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 4L, 8L, 0L, false, null, null, 1, 9500.0, 500, 100 },
                    { 8L, 5, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 4L, 9L, 0L, false, null, null, 1, 14800.0, 500, 100 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "Prod_Spec_Types",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "FK_Pid", "FK_S1id", "FK_S2id", "Price" },
                values: new object[] { 2L, 1L, 4L, 9500.0 });

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "FK_Pid", "Price" },
                values: new object[] { 3L, 13000.0 });

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 1L,
                column: "permanent",
                value: false);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 2L,
                column: "permanent",
                value: false);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 3L,
                column: "permanent",
                value: false);
        }
    }
}
