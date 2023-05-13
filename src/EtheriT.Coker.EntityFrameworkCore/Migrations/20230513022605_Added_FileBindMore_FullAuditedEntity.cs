using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_FileBindMore_FullAuditedEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "FileBindMores",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "CreatorUserId",
                table: "FileBindMores",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DeleterUserId",
                table: "FileBindMores",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "FileBindMores",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FileBindMores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "FileBindMores",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastModifierUserId",
                table: "FileBindMores",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "FileBindMores");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "FileBindMores");

            migrationBuilder.DropColumn(
                name: "DeleterUserId",
                table: "FileBindMores");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "FileBindMores");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "FileBindMores");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "FileBindMores");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "FileBindMores");
        }
    }
}
