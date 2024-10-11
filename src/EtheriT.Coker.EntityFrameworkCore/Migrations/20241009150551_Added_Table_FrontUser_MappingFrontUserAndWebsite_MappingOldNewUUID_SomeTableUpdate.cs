using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Table_FrontUser_MappingFrontUserAndWebsite_MappingOldNewUUID_SomeTableUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MappingFrontUserAndWebsiteId",
                table: "Remotes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MappingFrontUserAndWebsiteId",
                table: "Prod_Logs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FrontUserId",
                table: "MappingUserAndRoles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFront",
                table: "MappingUserAndRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "MappingFrontUserAndWebsiteId",
                table: "Advertise_Logs",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FrontUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Account = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CellPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TelPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Sex = table.Column<int>(type: "int", nullable: true),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorTimes = table.Column<int>(type: "int", nullable: false),
                    LockTime = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_FrontUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MappingOldNewUUID",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OldUUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NewUUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_MappingOldNewUUID", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MappingFrontUserAndWebsite",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_UserId = table.Column<long>(type: "bigint", nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OpenDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpenID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpenIDSendDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_MappingFrontUserAndWebsite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MappingFrontUserAndWebsite_FrontUsers_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "FrontUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MappingFrontUserAndWebsite_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_MappingFrontUserAndWebsiteId",
                table: "Remotes",
                column: "MappingFrontUserAndWebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Logs_MappingFrontUserAndWebsiteId",
                table: "Prod_Logs",
                column: "MappingFrontUserAndWebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingUserAndRoles_FrontUserId",
                table: "MappingUserAndRoles",
                column: "FrontUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertise_Logs_MappingFrontUserAndWebsiteId",
                table: "Advertise_Logs",
                column: "MappingFrontUserAndWebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingFrontUserAndWebsite_FK_UserId",
                table: "MappingFrontUserAndWebsite",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingFrontUserAndWebsite_FK_WebsiteId",
                table: "MappingFrontUserAndWebsite",
                column: "FK_WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertise_Logs_MappingFrontUserAndWebsite_MappingFrontUserAndWebsiteId",
                table: "Advertise_Logs",
                column: "MappingFrontUserAndWebsiteId",
                principalTable: "MappingFrontUserAndWebsite",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MappingUserAndRoles_FrontUsers_FrontUserId",
                table: "MappingUserAndRoles",
                column: "FrontUserId",
                principalTable: "FrontUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_Logs_MappingFrontUserAndWebsite_MappingFrontUserAndWebsiteId",
                table: "Prod_Logs",
                column: "MappingFrontUserAndWebsiteId",
                principalTable: "MappingFrontUserAndWebsite",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Remotes_MappingFrontUserAndWebsite_MappingFrontUserAndWebsiteId",
                table: "Remotes",
                column: "MappingFrontUserAndWebsiteId",
                principalTable: "MappingFrontUserAndWebsite",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertise_Logs_MappingFrontUserAndWebsite_MappingFrontUserAndWebsiteId",
                table: "Advertise_Logs");

            migrationBuilder.DropForeignKey(
                name: "FK_MappingUserAndRoles_FrontUsers_FrontUserId",
                table: "MappingUserAndRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Logs_MappingFrontUserAndWebsite_MappingFrontUserAndWebsiteId",
                table: "Prod_Logs");

            migrationBuilder.DropForeignKey(
                name: "FK_Remotes_MappingFrontUserAndWebsite_MappingFrontUserAndWebsiteId",
                table: "Remotes");

            migrationBuilder.DropTable(
                name: "MappingFrontUserAndWebsite");

            migrationBuilder.DropTable(
                name: "MappingOldNewUUID");

            migrationBuilder.DropTable(
                name: "FrontUsers");

            migrationBuilder.DropIndex(
                name: "IX_Remotes_MappingFrontUserAndWebsiteId",
                table: "Remotes");

            migrationBuilder.DropIndex(
                name: "IX_Prod_Logs_MappingFrontUserAndWebsiteId",
                table: "Prod_Logs");

            migrationBuilder.DropIndex(
                name: "IX_MappingUserAndRoles_FrontUserId",
                table: "MappingUserAndRoles");

            migrationBuilder.DropIndex(
                name: "IX_Advertise_Logs_MappingFrontUserAndWebsiteId",
                table: "Advertise_Logs");

            migrationBuilder.DropColumn(
                name: "MappingFrontUserAndWebsiteId",
                table: "Remotes");

            migrationBuilder.DropColumn(
                name: "MappingFrontUserAndWebsiteId",
                table: "Prod_Logs");

            migrationBuilder.DropColumn(
                name: "FrontUserId",
                table: "MappingUserAndRoles");

            migrationBuilder.DropColumn(
                name: "IsFront",
                table: "MappingUserAndRoles");

            migrationBuilder.DropColumn(
                name: "MappingFrontUserAndWebsiteId",
                table: "Advertise_Logs");
        }
    }
}
