using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_Remode_FK_TechCertId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FK_TechCertId",
                table: "Remotes",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_FK_TechCertId",
                table: "Remotes",
                column: "FK_TechCertId");

            migrationBuilder.AddForeignKey(
                name: "FK_Remotes_TechnicalCertificates_FK_TechCertId",
                table: "Remotes",
                column: "FK_TechCertId",
                principalTable: "TechnicalCertificates",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Remotes_TechnicalCertificates_FK_TechCertId",
                table: "Remotes");

            migrationBuilder.DropIndex(
                name: "IX_Remotes_FK_TechCertId",
                table: "Remotes");

            migrationBuilder.DropColumn(
                name: "FK_TechCertId",
                table: "Remotes");
        }
    }
}
