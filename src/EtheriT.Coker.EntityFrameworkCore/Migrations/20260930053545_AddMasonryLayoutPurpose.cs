using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddMasonryLayoutPurpose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ComponentPurposes",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "Name", "SerNo", "Visible" },
                values: new object[] { 2L, "imglist-layout-change", new DateTime(2026, 9, 30, 0, 0, 0, 0, DateTimeKind.Local), 2L, null, null, null, null, "相簿排版切換", 20, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ComponentPurposes",
                keyColumn: "Id",
                keyValue: 2L);
        }
    }
}
