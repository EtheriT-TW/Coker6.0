using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_MappingOldNewUUID_Rename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OldUUID",
                table: "MappingOldNewUUID",
                newName: "UserUUID");

            migrationBuilder.RenameColumn(
                name: "NewUUID",
                table: "MappingOldNewUUID",
                newName: "TempUUID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserUUID",
                table: "MappingOldNewUUID",
                newName: "OldUUID");

            migrationBuilder.RenameColumn(
                name: "TempUUID",
                table: "MappingOldNewUUID",
                newName: "NewUUID");
        }
    }
}
