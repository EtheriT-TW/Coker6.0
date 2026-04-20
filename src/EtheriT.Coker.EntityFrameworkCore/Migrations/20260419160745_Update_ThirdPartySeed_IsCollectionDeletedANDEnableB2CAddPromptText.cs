using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Update_ThirdPartySeed_IsCollectionDeletedANDEnableB2CAddPromptText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 22L,
                column: "PromptText",
                value: "如需啟用請記得至綠界後台測標");

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 24L,
                column: "IsDeleted",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 22L,
                column: "PromptText",
                value: null);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 24L,
                column: "IsDeleted",
                value: false);
        }
    }
}
