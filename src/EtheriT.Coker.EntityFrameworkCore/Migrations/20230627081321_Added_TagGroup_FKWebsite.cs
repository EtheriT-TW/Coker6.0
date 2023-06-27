using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_TagGroup_FKWebsite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FK_WebsiteId",
                table: "Tag_Groups",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FK_WebsiteId",
                table: "Tag_Groups");
        }
    }
}
