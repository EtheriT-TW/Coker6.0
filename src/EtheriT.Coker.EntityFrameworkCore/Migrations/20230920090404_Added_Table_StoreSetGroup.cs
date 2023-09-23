using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Table_StoreSetGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "groupType",
                table: "StoreSet");

            migrationBuilder.AlterColumn<string>(
                name: "job_id",
                table: "StoreSetDetail",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FK_StoreSetGroupId",
                table: "StoreSet",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "StoreSetGroup",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreSetGroup", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "StoreSetGroup",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Description", "Image", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[] { 1L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "", "/images/icon_google.png", false, null, null, "Google設定" });

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatorUserId", "FK_StoreSetGroupId", "key" },
                values: new object[] { 1L, 1L, "GA4" });

            migrationBuilder.CreateIndex(
                name: "IX_StoreSet_FK_StoreSetGroupId",
                table: "StoreSet",
                column: "FK_StoreSetGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreSet_StoreSetGroup_FK_StoreSetGroupId",
                table: "StoreSet",
                column: "FK_StoreSetGroupId",
                principalTable: "StoreSetGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreSet_StoreSetGroup_FK_StoreSetGroupId",
                table: "StoreSet");

            migrationBuilder.DropTable(
                name: "StoreSetGroup");

            migrationBuilder.DropIndex(
                name: "IX_StoreSet_FK_StoreSetGroupId",
                table: "StoreSet");

            migrationBuilder.DropColumn(
                name: "FK_StoreSetGroupId",
                table: "StoreSet");

            migrationBuilder.AlterColumn<string>(
                name: "job_id",
                table: "StoreSetDetail",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "groupType",
                table: "StoreSet",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatorUserId", "groupType", "key" },
                values: new object[] { 2L, 1, "Google" });
        }
    }
}
