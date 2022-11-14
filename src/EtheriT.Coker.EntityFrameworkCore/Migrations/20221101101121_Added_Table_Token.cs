using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Table_Token : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ip = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_Tokens_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "MappingUserAndWebsites",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 18, 11, 21, 208, DateTimeKind.Local).AddTicks(1688));

            migrationBuilder.UpdateData(
                table: "MappingUserAndWebsites",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 18, 11, 21, 208, DateTimeKind.Local).AddTicks(1689));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 18, 11, 21, 208, DateTimeKind.Local).AddTicks(1576));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 18, 11, 21, 208, DateTimeKind.Local).AddTicks(1586));

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 18, 11, 21, 208, DateTimeKind.Local).AddTicks(1676));

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 18, 11, 21, 208, DateTimeKind.Local).AddTicks(1677));

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_UserID",
                table: "Tokens",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.UpdateData(
                table: "MappingUserAndWebsites",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 17, 1, 23, 702, DateTimeKind.Local).AddTicks(8672));

            migrationBuilder.UpdateData(
                table: "MappingUserAndWebsites",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 17, 1, 23, 702, DateTimeKind.Local).AddTicks(8673));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 17, 1, 23, 702, DateTimeKind.Local).AddTicks(8560));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 17, 1, 23, 702, DateTimeKind.Local).AddTicks(8568));

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 17, 1, 23, 702, DateTimeKind.Local).AddTicks(8659));

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 17, 1, 23, 702, DateTimeKind.Local).AddTicks(8661));
        }
    }
}
