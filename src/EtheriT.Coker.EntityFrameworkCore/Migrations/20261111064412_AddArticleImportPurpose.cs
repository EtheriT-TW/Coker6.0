using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleImportPurpose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ComponentPurposes",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "Name", "SerNo", "Visible" },
                values: new object[] { 3L, "article-import-directory", new DateTime(2026, 11, 11, 0, 0, 0, 0, DateTimeKind.Local), 2L, null, null, null, null, "文章匯入目錄", 30, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ComponentPurposes",
                keyColumn: "Id",
                keyValue: 3L);
        }
    }
}
