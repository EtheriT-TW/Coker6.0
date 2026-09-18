using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformWebsiteLinkedSiteIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 這個索引在部分環境已被手動建立（migration 檔案遺失、資料庫卻已套用），
            // 先移除再重建，確保不論起始狀態為何都能跑過。
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE name = 'IX_PlatformWebsites_FK_WebsiteId'
             AND object_id = OBJECT_ID('PlatformWebsites'))
    DROP INDEX [IX_PlatformWebsites_FK_WebsiteId] ON [PlatformWebsites];");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWebsites_FK_WebsiteId",
                table: "PlatformWebsites",
                column: "FK_WebsiteId",
                unique: true,
                filter: "[FK_WebsiteId] IS NOT NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlatformWebsites_FK_WebsiteId",
                table: "PlatformWebsites");
        }
    }
}
