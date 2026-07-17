using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    [DbContext(typeof(CokerDbContext))]
    [Migration("20260716073000_AddShoppingCartOrderSnapshots")]
    public partial class AddShoppingCartOrderSnapshots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ProductId",
                table: "ShoppingCarts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "S1Title",
                table: "ShoppingCarts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "S2Title",
                table: "ShoppingCarts",
                type: "nvarchar(max)",
                nullable: true);

            // 直接由資料表回填，不會受 EF Core 軟刪除 Query Filter 影響。
            migrationBuilder.Sql(
                """
                UPDATE sc
                SET
                    sc.ProductId = ps.FK_Pid,
                    sc.ProdName = COALESCE(NULLIF(sc.ProdName, ''), p.Title),
                    sc.S1Title = s1.Title,
                    sc.S2Title = s2.Title
                FROM ShoppingCarts AS sc
                LEFT JOIN Prod_Stocks AS ps ON ps.Id = sc.FK_PSid
                LEFT JOIN Prods AS p ON p.Id = ps.FK_Pid
                LEFT JOIN Prod_Specs AS s1 ON s1.Id = COALESCE(sc.FK_S1id, ps.FK_S1id)
                LEFT JOIN Prod_Specs AS s2 ON s2.Id = COALESCE(sc.FK_S2id, ps.FK_S2id)
                WHERE ps.Id IS NOT NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "S1Title",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "S2Title",
                table: "ShoppingCarts");
        }
    }
}
