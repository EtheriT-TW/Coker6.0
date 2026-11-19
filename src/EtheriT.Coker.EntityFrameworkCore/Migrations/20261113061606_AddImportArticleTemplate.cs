using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddImportArticleTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ComponentPurposes",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "Name", "SerNo", "Visible" },
                values: new object[] { 4L, "article-import-template", new DateTime(2026, 11, 13, 0, 0, 0, 0, DateTimeKind.Local), 2L, null, null, null, null, "匯入用文章樣板", 40, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ComponentPurposes",
                keyColumn: "Id",
                keyValue: 4L);
        }
    }
}
