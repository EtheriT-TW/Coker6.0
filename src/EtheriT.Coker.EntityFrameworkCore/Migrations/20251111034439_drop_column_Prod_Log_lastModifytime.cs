using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class drop_column_Prod_Log_lastModifytime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "Prod_Logs");

            migrationBuilder.DropColumn(
                name: "Db_Name",
                table: "Prod_Logs");

            migrationBuilder.DropColumn(
                name: "DeleterUserId",
                table: "Prod_Logs");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Prod_Logs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Prod_Logs");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "Prod_Logs");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "Prod_Logs");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationTime",
                table: "Prod_Logs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "getdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "Prod_Logs",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Remark",
                table: "Prod_Logs");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationTime",
                table: "Prod_Logs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "getdate()");

            migrationBuilder.AddColumn<long>(
                name: "CreatorUserId",
                table: "Prod_Logs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Db_Name",
                table: "Prod_Logs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeleterUserId",
                table: "Prod_Logs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Prod_Logs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Prod_Logs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "Prod_Logs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastModifierUserId",
                table: "Prod_Logs",
                type: "bigint",
                nullable: true);
        }
    }
}
