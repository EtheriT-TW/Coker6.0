using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_MappingFrontUserAndWebsite_Forget : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForgeIDSendDate",
                table: "MappingFrontUserAndWebsite");

            migrationBuilder.DropColumn(
                name: "ForgetID",
                table: "MappingFrontUserAndWebsite");
        }
    }
}
