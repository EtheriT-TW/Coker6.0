using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_Account_Log_Remove_MappingFrontUserAndWebsite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_Logs_MappingFrontUserAndWebsite_mappingFrontUserAndWebsiteId",
                table: "Account_Logs");

            migrationBuilder.DropIndex(
                name: "IX_Account_Logs_mappingFrontUserAndWebsiteId",
                table: "Account_Logs");

            migrationBuilder.DropColumn(
                name: "mappingFrontUserAndWebsiteId",
                table: "Account_Logs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "mappingFrontUserAndWebsiteId",
                table: "Account_Logs",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_Logs_mappingFrontUserAndWebsiteId",
                table: "Account_Logs",
                column: "mappingFrontUserAndWebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Account_Logs_MappingFrontUserAndWebsite_mappingFrontUserAndWebsiteId",
                table: "Account_Logs",
                column: "mappingFrontUserAndWebsiteId",
                principalTable: "MappingFrontUserAndWebsite",
                principalColumn: "Id");
        }
    }
}
