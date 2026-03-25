using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Added_WebsiteCacheState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JsonObjects_FK_WebsiteId",
                table: "JsonObjects");

            migrationBuilder.Sql(@"
                ;WITH CTE AS
                (
                    SELECT 
                        Id,
                        FK_WebsiteId,
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY FK_WebsiteId
                            ORDER BY ISNULL(LastModificationTime, CreationTime) DESC, Id DESC
                        ) AS rn
                    FROM JsonObjects
                )
                DELETE FROM CTE
                WHERE rn > 1;
            ");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "JsonObjects");

            migrationBuilder.AddColumn<string>(
                name: "CacheKey",
                table: "JsonObjects",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "menu");

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "JsonObjects",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "WebsiteCacheStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    CacheKey = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteCacheStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebsiteCacheStates_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JsonObjects_FK_WebsiteId_CacheKey",
                table: "JsonObjects",
                columns: new[] { "FK_WebsiteId", "CacheKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteCacheStates_FK_WebsiteId_CacheKey",
                table: "WebsiteCacheStates",
                columns: new[] { "FK_WebsiteId", "CacheKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebsiteCacheStates");

            migrationBuilder.DropIndex(
                name: "IX_JsonObjects_FK_WebsiteId_CacheKey",
                table: "JsonObjects");

            migrationBuilder.DropColumn(
                name: "CacheKey",
                table: "JsonObjects");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "JsonObjects");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "JsonObjects",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_JsonObjects_FK_WebsiteId",
                table: "JsonObjects",
                column: "FK_WebsiteId");
        }
    }
}
