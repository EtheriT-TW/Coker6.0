using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Append_ObjectType_13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ObjectTypes",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "SerNo", "Title" },
                values: new object[] { 13L, new DateTime(2026, 3, 4, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "文章樣板" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 13L);
        }
    }
}
