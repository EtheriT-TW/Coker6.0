using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_ECPay_Seed_RemoveExtraSpace : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 11L,
                column: "Code",
                value: "HashKey");

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 12L,
                column: "Code",
                value: "HashIV");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 11L,
                column: "Code",
                value: "HashKey ");

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 12L,
                column: "Code",
                value: "HashIV ");
        }
    }
}
