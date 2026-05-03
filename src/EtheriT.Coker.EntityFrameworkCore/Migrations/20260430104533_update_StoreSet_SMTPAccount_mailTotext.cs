using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class update_StoreSet_SMTPAccount_mailTotext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 11L,
                columns: new[] { "maxlength", "memo", "type" },
                values: new object[] { 100, "請輸入 SMTP 帳號；若不是 Email，系統將使用客服信箱作為寄件人", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 11L,
                columns: new[] { "maxlength", "memo", "type" },
                values: new object[] { 255, "請輸入 帳號", 9 });
        }
    }
}
