using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateECPayMemo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 4L,
                column: "Memo",
                value: "Apple pay 、 URICH 、 ZINGALA 、 TWQR須再跟綠界開通服務，並請洽詢網站平台業務單位加購服務設定。");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 4L,
                column: "Memo",
                value: "Apple pay 須再跟綠界開通服務，並請洽詢網站平台業務單位加購服務設定。");
        }
    }
}
