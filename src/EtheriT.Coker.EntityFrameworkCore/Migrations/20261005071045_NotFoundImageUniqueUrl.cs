using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class NotFoundImageUniqueUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotFoundImage_FK_WebsiteId",
                table: "NotFoundImage");

            migrationBuilder.Sql(@"
                WITH d AS (
                    SELECT Id, ROW_NUMBER() OVER (PARTITION BY FK_WebsiteId, Url ORDER BY Id) AS rn
                    FROM NotFoundImage
                )
                DELETE FROM d WHERE rn > 1;");

            migrationBuilder.CreateIndex(
                name: "IX_NotFoundImage_FK_WebsiteId_Url",
                table: "NotFoundImage",
                columns: new[] { "FK_WebsiteId", "Url" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotFoundImage_FK_WebsiteId_Url",
                table: "NotFoundImage");

            migrationBuilder.CreateIndex(
                name: "IX_NotFoundImage_FK_WebsiteId",
                table: "NotFoundImage",
                column: "FK_WebsiteId");
        }
    }
}
