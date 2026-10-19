using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class MoveEnterpriseEmailToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebsiteEnterpriseEmails_Websites_FK_WebsiteId",
                table: "WebsiteEnterpriseEmails");

            migrationBuilder.DropIndex(
                name: "IX_WebsiteEnterpriseEmails_FK_WebsiteId",
                table: "WebsiteEnterpriseEmails");

            // 企業信箱改綁客戶，舊資料存的是站台 Id，無法對應到 Companies，直接清空（正式機無資料）
            migrationBuilder.Sql("DELETE FROM WebsiteEnterpriseEmails;");

            migrationBuilder.RenameColumn(
                name: "FK_WebsiteId",
                table: "WebsiteEnterpriseEmails",
                newName: "FK_CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteEnterpriseEmails_FK_CompanyId",
                table: "WebsiteEnterpriseEmails",
                column: "FK_CompanyId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_WebsiteEnterpriseEmails_Companies_FK_CompanyId",
                table: "WebsiteEnterpriseEmails",
                column: "FK_CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebsiteEnterpriseEmails_Companies_FK_CompanyId",
                table: "WebsiteEnterpriseEmails");

            migrationBuilder.DropIndex(
                name: "IX_WebsiteEnterpriseEmails_FK_CompanyId",
                table: "WebsiteEnterpriseEmails");

            migrationBuilder.RenameColumn(
                name: "FK_CompanyId",
                table: "WebsiteEnterpriseEmails",
                newName: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteEnterpriseEmails_FK_WebsiteId",
                table: "WebsiteEnterpriseEmails",
                column: "FK_WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_WebsiteEnterpriseEmails_Websites_FK_WebsiteId",
                table: "WebsiteEnterpriseEmails",
                column: "FK_WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
