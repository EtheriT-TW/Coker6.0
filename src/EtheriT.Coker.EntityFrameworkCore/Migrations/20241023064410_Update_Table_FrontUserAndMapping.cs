using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_FrontUserAndMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertise_Logs_MappingFrontUserAndWebsite_MappingFrontUserAndWebsiteId",
                table: "Advertise_Logs");

            migrationBuilder.DropForeignKey(
                name: "FK_MappingFrontUserAndRoles_FrontUsers_FrontUserId",
                table: "MappingFrontUserAndRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_MappingFrontUserAndRoles_Roles_RoleId",
                table: "MappingFrontUserAndRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Logs_MappingFrontUserAndWebsite_MappingFrontUserAndWebsiteId",
                table: "Prod_Logs");

            migrationBuilder.DropForeignKey(
                name: "FK_Remotes_MappingFrontUserAndWebsite_MappingFrontUserAndWebsiteId",
                table: "Remotes");

            migrationBuilder.DropIndex(
                name: "IX_Remotes_MappingFrontUserAndWebsiteId",
                table: "Remotes");

            migrationBuilder.DropIndex(
                name: "IX_Prod_Logs_MappingFrontUserAndWebsiteId",
                table: "Prod_Logs");

            migrationBuilder.DropIndex(
                name: "IX_MappingFrontUserAndRoles_FrontUserId",
                table: "MappingFrontUserAndRoles");

            migrationBuilder.DropIndex(
                name: "IX_MappingFrontUserAndRoles_RoleId",
                table: "MappingFrontUserAndRoles");

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
                name: "ForgeIDSendDate",
                table: "MappingFrontUserAndWebsite");

            migrationBuilder.DropColumn(
                name: "ForgetID",
                table: "MappingFrontUserAndWebsite");

            migrationBuilder.DropColumn(
                name: "OpenDate",
                table: "MappingFrontUserAndWebsite");

            migrationBuilder.DropColumn(
                name: "OpenID",
                table: "MappingFrontUserAndWebsite");

            migrationBuilder.DropColumn(
                name: "OpenIDSendDate",
                table: "MappingFrontUserAndWebsite");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MappingFrontUserAndWebsite");

            migrationBuilder.DropColumn(
                name: "UUID",
                table: "MappingFrontUserAndWebsite");

            migrationBuilder.DropColumn(
                name: "FrontUserId",
                table: "MappingFrontUserAndRoles");

            migrationBuilder.DropColumn(
                name: "UUID",
                table: "MappingFrontUserAndRoles");

            migrationBuilder.DropColumn(
                name: "MappingFrontUserAndWebsiteId",
                table: "Advertise_Logs");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "MappingFrontUserAndRoles",
                newName: "FK_UserId");

            migrationBuilder.AddColumn<long>(
                name: "FK_RoleId",
                table: "MappingFrontUserAndRoles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "ForgeIDSendDate",
                table: "FrontUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ForgetID",
                table: "FrontUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenDate",
                table: "FrontUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OpenID",
                table: "FrontUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenIDSendDate",
                table: "FrontUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "FrontUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UUID",
                table: "FrontUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_MappingFrontUserAndRoles_FK_RoleId",
                table: "MappingFrontUserAndRoles",
                column: "FK_RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingFrontUserAndRoles_UserId",
                table: "MappingFrontUserAndRoles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MappingFrontUserAndRoles_FrontUsers_UserId",
                table: "MappingFrontUserAndRoles",
                column: "UserId",
                principalTable: "FrontUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MappingFrontUserAndRoles_Roles_FK_RoleId",
                table: "MappingFrontUserAndRoles",
                column: "FK_RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MappingFrontUserAndRoles_FrontUsers_UserId",
                table: "MappingFrontUserAndRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_MappingFrontUserAndRoles_Roles_FK_RoleId",
                table: "MappingFrontUserAndRoles");

            migrationBuilder.DropIndex(
                name: "IX_MappingFrontUserAndRoles_FK_RoleId",
                table: "MappingFrontUserAndRoles");

            migrationBuilder.DropIndex(
                name: "IX_MappingFrontUserAndRoles_UserId",
                table: "MappingFrontUserAndRoles");

            migrationBuilder.DropColumn(
                name: "FK_RoleId",
                table: "MappingFrontUserAndRoles");

            migrationBuilder.DropColumn(
                name: "ForgeIDSendDate",
                table: "FrontUsers");

            migrationBuilder.DropColumn(
                name: "ForgetID",
                table: "FrontUsers");

            migrationBuilder.DropColumn(
                name: "OpenDate",
                table: "FrontUsers");

            migrationBuilder.DropColumn(
                name: "OpenID",
                table: "FrontUsers");

            migrationBuilder.DropColumn(
                name: "OpenIDSendDate",
                table: "FrontUsers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "FrontUsers");

            migrationBuilder.DropColumn(
                name: "UUID",
                table: "FrontUsers");

            migrationBuilder.RenameColumn(
                name: "FK_UserId",
                table: "MappingFrontUserAndRoles",
                newName: "RoleId");

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

            migrationBuilder.AddColumn<DateTime>(
                name: "ForgeIDSendDate",
                table: "MappingFrontUserAndWebsite",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ForgetID",
                table: "MappingFrontUserAndWebsite",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenDate",
                table: "MappingFrontUserAndWebsite",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OpenID",
                table: "MappingFrontUserAndWebsite",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenIDSendDate",
                table: "MappingFrontUserAndWebsite",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "MappingFrontUserAndWebsite",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UUID",
                table: "MappingFrontUserAndWebsite",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "FrontUserId",
                table: "MappingFrontUserAndRoles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UUID",
                table: "MappingFrontUserAndRoles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "MappingFrontUserAndWebsiteId",
                table: "Advertise_Logs",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_MappingFrontUserAndWebsiteId",
                table: "Remotes",
                column: "MappingFrontUserAndWebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Logs_MappingFrontUserAndWebsiteId",
                table: "Prod_Logs",
                column: "MappingFrontUserAndWebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingFrontUserAndRoles_FrontUserId",
                table: "MappingFrontUserAndRoles",
                column: "FrontUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingFrontUserAndRoles_RoleId",
                table: "MappingFrontUserAndRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertise_Logs_MappingFrontUserAndWebsiteId",
                table: "Advertise_Logs",
                column: "MappingFrontUserAndWebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertise_Logs_MappingFrontUserAndWebsite_MappingFrontUserAndWebsiteId",
                table: "Advertise_Logs",
                column: "MappingFrontUserAndWebsiteId",
                principalTable: "MappingFrontUserAndWebsite",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MappingFrontUserAndRoles_FrontUsers_FrontUserId",
                table: "MappingFrontUserAndRoles",
                column: "FrontUserId",
                principalTable: "FrontUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MappingFrontUserAndRoles_Roles_RoleId",
                table: "MappingFrontUserAndRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
    }
}
