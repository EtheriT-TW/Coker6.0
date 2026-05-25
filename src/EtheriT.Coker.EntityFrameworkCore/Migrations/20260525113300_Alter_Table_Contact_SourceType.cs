using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Table_Contact_SourceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FK_SourceId",
                table: "Contacts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                table: "Contacts",
                type: "int",
                nullable: true);

            var menuSourceType = (int)HtmlSanitizeSourceType.選單;

            migrationBuilder.Sql($@"
                UPDATE Contacts
                SET
                    FK_SourceId = FK_WebMenuId,
                    SourceType = {menuSourceType}
                WHERE
                    FK_SourceId IS NULL
                    OR SourceType IS NULL
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FK_SourceId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "Contacts");
        }
    }
}
