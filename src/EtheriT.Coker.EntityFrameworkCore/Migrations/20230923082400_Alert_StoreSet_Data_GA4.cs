using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alert_StoreSet_Data_GA4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "maxlength", "memo" },
                values: new object[] { 12, "請輸入GOOGLE提供之驗證碼：G-xxxxxxxxxx" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "maxlength", "memo" },
                values: new object[] { 8, "請輸入GOOGLE提供之驗證碼：xxxxxx-x" });
        }
    }
}
