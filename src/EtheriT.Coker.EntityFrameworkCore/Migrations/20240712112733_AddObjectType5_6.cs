using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class AddObjectType5_6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ObjectTypes",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "IsDeleted", "LastModificationTime", "LastModifierUserId", "SerNo", "Title" },
                values: new object[] { 5L, new DateTime(2024, 7, 12, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, false, null, null, 500, "樣版" });

            migrationBuilder.InsertData(
                table: "ObjectTypes",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "IsDeleted", "LastModificationTime", "LastModifierUserId", "SerNo", "Title" },
                values: new object[] { 6L, new DateTime(2024, 7, 12, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, false, null, null, 500, "框架" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 6L);
        }
    }
}
