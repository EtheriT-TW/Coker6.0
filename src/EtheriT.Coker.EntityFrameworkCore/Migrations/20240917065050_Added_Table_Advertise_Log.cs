using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Table_Advertise_Log : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Advertise_Logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_Adid = table.Column<long>(type: "bigint", nullable: false),
                    FK_Tid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_Uid = table.Column<long>(type: "bigint", nullable: true),
                    Action = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Advertise_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Advertise_Logs_Advertise_FK_Adid",
                        column: x => x.FK_Adid,
                        principalTable: "Advertise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Advertise_Logs_Tokens_FK_Tid",
                        column: x => x.FK_Tid,
                        principalTable: "Tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Advertise_Logs_Users_FK_Uid",
                        column: x => x.FK_Uid,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Advertise_Logs_FK_Adid",
                table: "Advertise_Logs",
                column: "FK_Adid");

            migrationBuilder.CreateIndex(
                name: "IX_Advertise_Logs_FK_Tid",
                table: "Advertise_Logs",
                column: "FK_Tid");

            migrationBuilder.CreateIndex(
                name: "IX_Advertise_Logs_FK_Uid",
                table: "Advertise_Logs",
                column: "FK_Uid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Advertise_Logs");
        }
    }
}
