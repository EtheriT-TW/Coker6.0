using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Update_storeSetItem_InvoiceMemo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 24L,
                column: "memo",
                value: "網站是否需要開立發票");

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 26L,
                column: "memo",
                value: "請輸入隱私聲明內文(本區塊支援 Markdown 標記語法，### 表示標題，**文字** 表示字粗體)");

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 15L,
                column: "Value",
                value: "是");

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 16L,
                column: "Value",
                value: "否");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 24L,
                column: "memo",
                value: "訂單是否供客戶選擇開立發票方式。");

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 26L,
                column: "memo",
                value: "請輸入隱私聲明內文");

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 15L,
                column: "Value",
                value: "允許");

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 16L,
                column: "Value",
                value: "不允許");
        }
    }
}
