using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Alter_ThirdParty_Memo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Memo",
                table: "ThirdParties",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Memo",
                value: null);

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 2L,
                column: "Memo",
                value: null);

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 3L,
                column: "Memo",
                value: null);

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 4L,
                column: "Memo",
                value: "Apple pay 須再跟綠界開通服務，並請洽詢網站平台業務單位加購服務設定。");

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 5L,
                column: "Memo",
                value: null);

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 6L,
                column: "Memo",
                value: null);

            migrationBuilder.UpdateData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 7L,
                column: "Memo",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Memo",
                table: "ThirdParties");
        }
    }
}
