using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Constraint_UserTagStatistic_Tag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTagStatistics_Tags_TagId",
                table: "UserTagStatistics");

            migrationBuilder.DropIndex(
                name: "IX_UserTagStatistics_TagId",
                table: "UserTagStatistics");

            migrationBuilder.DropColumn(
                name: "TagId",
                table: "UserTagStatistics");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModificationTime",
                table: "UserTagStatistics",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "getdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastActivityTime",
                table: "UserTagStatistics",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "getdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_UserTagStatistics_FK_TagId",
                table: "UserTagStatistics",
                column: "FK_TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTagStatistics_Tags_FK_TagId",
                table: "UserTagStatistics",
                column: "FK_TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTagStatistics_Tags_FK_TagId",
                table: "UserTagStatistics");

            migrationBuilder.DropIndex(
                name: "IX_UserTagStatistics_FK_TagId",
                table: "UserTagStatistics");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModificationTime",
                table: "UserTagStatistics",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "getdate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastActivityTime",
                table: "UserTagStatistics",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "getdate()");

            migrationBuilder.AddColumn<long>(
                name: "TagId",
                table: "UserTagStatistics",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_UserTagStatistics_TagId",
                table: "UserTagStatistics",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTagStatistics_Tags_TagId",
                table: "UserTagStatistics",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
