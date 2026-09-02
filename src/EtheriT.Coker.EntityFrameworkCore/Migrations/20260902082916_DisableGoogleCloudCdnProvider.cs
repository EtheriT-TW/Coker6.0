using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class DisableGoogleCloudCdnProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 26L,
                columns: new[] { "DeleterUserId", "DeletionTime", "IsDeleted" },
                values: new object[] { 1L, new DateTime(2026, 9, 2, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 26L,
                columns: new[] { "DeleterUserId", "DeletionTime", "IsDeleted" },
                values: new object[] { null, null, false });
        }
    }
}
