using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alert_table_ThirdPartyKeypairValue_WebsiteId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTypesValues_Websites_websiteId",
                table: "PaymentTypesValues");

            migrationBuilder.DropForeignKey(
                name: "FK_ThirdPartyKeypairValues_Websites_WebsiteId",
                table: "ThirdPartyKeypairValues");

            migrationBuilder.DropIndex(
                name: "IX_ThirdPartyKeypairValues_WebsiteId",
                table: "ThirdPartyKeypairValues");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTypesValues_websiteId",
                table: "PaymentTypesValues");

            migrationBuilder.DropColumn(
                name: "WebsiteId",
                table: "ThirdPartyKeypairValues");

            migrationBuilder.DropColumn(
                name: "websiteId",
                table: "PaymentTypesValues");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdPartyKeypairValues_FK_WebsiteId",
                table: "ThirdPartyKeypairValues",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypesValues_FK_WebsiteId",
                table: "PaymentTypesValues",
                column: "FK_WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTypesValues_Websites_FK_WebsiteId",
                table: "PaymentTypesValues",
                column: "FK_WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThirdPartyKeypairValues_Websites_FK_WebsiteId",
                table: "ThirdPartyKeypairValues",
                column: "FK_WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTypesValues_Websites_FK_WebsiteId",
                table: "PaymentTypesValues");

            migrationBuilder.DropForeignKey(
                name: "FK_ThirdPartyKeypairValues_Websites_FK_WebsiteId",
                table: "ThirdPartyKeypairValues");

            migrationBuilder.DropIndex(
                name: "IX_ThirdPartyKeypairValues_FK_WebsiteId",
                table: "ThirdPartyKeypairValues");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTypesValues_FK_WebsiteId",
                table: "PaymentTypesValues");

            migrationBuilder.AddColumn<long>(
                name: "WebsiteId",
                table: "ThirdPartyKeypairValues",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "websiteId",
                table: "PaymentTypesValues",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_ThirdPartyKeypairValues_WebsiteId",
                table: "ThirdPartyKeypairValues",
                column: "WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypesValues_websiteId",
                table: "PaymentTypesValues",
                column: "websiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTypesValues_Websites_websiteId",
                table: "PaymentTypesValues",
                column: "websiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThirdPartyKeypairValues_Websites_WebsiteId",
                table: "ThirdPartyKeypairValues",
                column: "WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
