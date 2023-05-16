using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_FileBind_FullAuditedEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "FileBinds",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "CreatorUserId",
                table: "FileBinds",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DeleterUserId",
                table: "FileBinds",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "FileBinds",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "FileBinds",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FileBinds",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "FileBinds",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastModifierUserId",
                table: "FileBinds",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FK_WebsiteId",
                table: "Article",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Article_FK_WebsiteId",
                table: "Article",
                column: "FK_WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Article_Websites_FK_WebsiteId",
                table: "Article",
                column: "FK_WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Article_Websites_FK_WebsiteId",
                table: "Article");

            migrationBuilder.DropIndex(
                name: "IX_Article_FK_WebsiteId",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "FileBinds");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "FileBinds");

            migrationBuilder.DropColumn(
                name: "DeleterUserId",
                table: "FileBinds");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "FileBinds");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FileBinds");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "FileBinds");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "FileBinds");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "FileBinds");

            migrationBuilder.DropColumn(
                name: "FK_WebsiteId",
                table: "Article");
        }
    }
}
