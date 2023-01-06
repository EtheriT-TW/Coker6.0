using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_FileBinds_FileBindMore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileBindMores",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_FileBindGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_FileUploadId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileBindMores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileBinds",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sid = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    num = table.Column<int>(type: "int", nullable: false),
                    SerNo = table.Column<int>(type: "int", nullable: false),
                    MediaLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FK_FileUploadId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileBinds", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_FileBinds_FileUploads_FK_FileUploadId",
                        column: x => x.FK_FileUploadId,
                        principalTable: "FileUploads",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileBinds_FK_FileUploadId",
                table: "FileBinds",
                column: "FK_FileUploadId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileBindMores");

            migrationBuilder.DropTable(
                name: "FileBinds");
        }
    }
}
