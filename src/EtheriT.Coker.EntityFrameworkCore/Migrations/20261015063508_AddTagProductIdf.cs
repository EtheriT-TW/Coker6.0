using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddTagProductIdf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ProductIdfUpdatedAtUtc",
                table: "Tags",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ProductIdfWeight",
                table: "Tags",
                type: "float",
                nullable: false,
                defaultValue: 1.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductIdfUpdatedAtUtc",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "ProductIdfWeight",
                table: "Tags");
        }
    }
}
