using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Update_StoreSet_prodCatalog_memo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 7L,
                column: "memo",
                value: "輸入商品目錄連結，可設定前台購物車(我要再選購)之按鈕。");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 7L,
                column: "memo",
                value: "輸入商品目錄連結，以利前台新增返回目錄按鈕。");
        }
    }
}
