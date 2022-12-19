using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Add_Table_Role_AND_ProdPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Safe_Qty",
                table: "Prod_Stocks",
                newName: "Alert_Qty");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prod_Prices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_PSId = table.Column<long>(type: "bigint", nullable: false),
                    FK_RId = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Bonus = table.Column<double>(type: "float", nullable: true),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prod_Prices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prod_Prices_Prod_Stocks_FK_PSId",
                        column: x => x.FK_PSId,
                        principalTable: "Prod_Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prod_Prices_Roles_FK_RId",
                        column: x => x.FK_RId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Prod_Spec_Types",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Type",
                value: "顏色");

            migrationBuilder.UpdateData(
                table: "Prod_Spec_Types",
                keyColumn: "Id",
                keyValue: 2L,
                column: "Type",
                value: "尺寸");

            migrationBuilder.UpdateData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Title",
                value: "白色");

            migrationBuilder.UpdateData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 2L,
                column: "Title",
                value: "灰色");

            migrationBuilder.UpdateData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 3L,
                column: "Title",
                value: "黑色");

            migrationBuilder.UpdateData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 4L,
                column: "Title",
                value: "小");

            migrationBuilder.UpdateData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 5L,
                column: "Title",
                value: "中");

            migrationBuilder.UpdateData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 6L,
                column: "Title",
                value: "大");

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "FK_S1id", "FK_S2id" },
                values: new object[] { 1L, 4L });

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "FK_Pid", "FK_S1id", "FK_S2id", "Price" },
                values: new object[] { 1L, 2L, 4L, 28000.0 });

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "FK_Pid", "FK_S1id", "FK_S2id", "Price" },
                values: new object[] { 2L, 1L, 4L, 9500.0 });

            migrationBuilder.InsertData(
                table: "Prod_Stocks",
                columns: new[] { "Id", "Alert_Qty", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_Pid", "FK_S1id", "FK_S2id", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Min_Qty", "Price", "Ser_No", "Stock" },
                values: new object[] { 4L, 5, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 3L, 1L, 4L, false, null, null, 1, 13000.0, 500, 100 });

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Discount",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Prices_FK_PSId",
                table: "Prod_Prices",
                column: "FK_PSId");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Prices_FK_RId",
                table: "Prod_Prices",
                column: "FK_RId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prod_Prices");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DeleteData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.RenameColumn(
                name: "Alert_Qty",
                table: "Prod_Stocks",
                newName: "Safe_Qty");

            migrationBuilder.UpdateData(
                table: "Prod_Spec_Types",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Type",
                value: "color");

            migrationBuilder.UpdateData(
                table: "Prod_Spec_Types",
                keyColumn: "Id",
                keyValue: 2L,
                column: "Type",
                value: "size");

            migrationBuilder.UpdateData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Title",
                value: "white");

            migrationBuilder.UpdateData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 2L,
                column: "Title",
                value: "gray");

            migrationBuilder.UpdateData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 3L,
                column: "Title",
                value: "black");

            migrationBuilder.UpdateData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 4L,
                column: "Title",
                value: "small");

            migrationBuilder.UpdateData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 5L,
                column: "Title",
                value: "medium");

            migrationBuilder.UpdateData(
                table: "Prod_Specs",
                keyColumn: "Id",
                keyValue: 6L,
                column: "Title",
                value: "large");

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "FK_S1id", "FK_S2id" },
                values: new object[] { 0L, 0L });

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "FK_Pid", "FK_S1id", "FK_S2id", "Price" },
                values: new object[] { 2L, 0L, 0L, 9500.0 });

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "FK_Pid", "FK_S1id", "FK_S2id", "Price" },
                values: new object[] { 3L, 0L, 0L, 13000.0 });

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Discount",
                value: 28000.0);
        }
    }
}
