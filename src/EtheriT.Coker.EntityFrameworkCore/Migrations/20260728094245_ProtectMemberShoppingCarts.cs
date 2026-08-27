using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class ProtectMemberShoppingCarts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM [sys].[indexes]
                    WHERE [object_id] = OBJECT_ID(N'[dbo].[MappingOldNewUUID]')
                      AND [name] = N'IX_MappingOldNewUUID_TempUUID_UserUUID'
                )
                BEGIN
                    CREATE INDEX [IX_MappingOldNewUUID_TempUUID_UserUUID]
                    ON [dbo].[MappingOldNewUUID] ([TempUUID], [UserUUID]);
                END;
                """,
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MappingOldNewUUID_TempUUID_UserUUID",
                table: "MappingOldNewUUID");
        }
    }
}
