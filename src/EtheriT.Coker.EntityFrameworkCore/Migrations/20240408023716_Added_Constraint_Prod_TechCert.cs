using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Constraint_Prod_TechCert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Prod_TechCerts_FK_PId",
                table: "Prod_TechCerts",
                column: "FK_PId");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_TechCerts_FK_TCId",
                table: "Prod_TechCerts",
                column: "FK_TCId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_TechCerts_Prods_FK_PId",
                table: "Prod_TechCerts",
                column: "FK_PId",
                principalTable: "Prods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_TechCerts_TechnicalCertificates_FK_TCId",
                table: "Prod_TechCerts",
                column: "FK_TCId",
                principalTable: "TechnicalCertificates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prod_TechCerts_Prods_FK_PId",
                table: "Prod_TechCerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Prod_TechCerts_TechnicalCertificates_FK_TCId",
                table: "Prod_TechCerts");

            migrationBuilder.DropIndex(
                name: "IX_Prod_TechCerts_FK_PId",
                table: "Prod_TechCerts");

            migrationBuilder.DropIndex(
                name: "IX_Prod_TechCerts_FK_TCId",
                table: "Prod_TechCerts");
        }
    }
}
