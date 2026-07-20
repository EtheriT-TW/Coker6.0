using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddDirectoryMenuJsonObjectIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JsonObjects_FK_WebsiteId_CacheKey",
                table: "JsonObjects");

            migrationBuilder.CreateIndex(
                name: "IX_JsonObjects_FK_WebsiteId_CacheKey_FK_AId",
                table: "JsonObjects",
                columns: new[] { "FK_WebsiteId", "CacheKey", "FK_AId" },
                unique: true,
                filter: "[FK_AId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JsonObjects_FK_WebsiteId_CacheKey_FK_AId",
                table: "JsonObjects");

            migrationBuilder.CreateIndex(
                name: "IX_JsonObjects_FK_WebsiteId_CacheKey",
                table: "JsonObjects",
                columns: new[] { "FK_WebsiteId", "CacheKey" },
                unique: true);
        }
    }
}
