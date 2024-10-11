using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_ResetTokenRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertise_Logs_Tokens_FK_Tid",
                table: "Advertise_Logs");

            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Logs_Tokens_FK_Tid",
                table: "Prod_Logs");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_Tokens_FK_Tid",
                table: "ShoppingCarts");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCarts_FK_Tid",
                table: "ShoppingCarts");

            migrationBuilder.DropIndex(
                name: "IX_Prod_Logs_FK_Tid",
                table: "Prod_Logs");

            migrationBuilder.DropIndex(
                name: "IX_Advertise_Logs_FK_Tid",
                table: "Advertise_Logs");

            migrationBuilder.CreateTable(
                name: "TokenMapAdvertise_Log",
                columns: table => new
                {
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_Tid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenMapAdvertise_Log", x => new { x.UUID, x.FK_Tid });
                    table.ForeignKey(
                        name: "FK_TokenMapAdvertise_Log_Advertise_Logs_FK_Tid",
                        column: x => x.FK_Tid,
                        principalTable: "Advertise_Logs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TokenMapAdvertise_Log_Tokens_UUID",
                        column: x => x.UUID,
                        principalTable: "Tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TokenMapProd_Log",
                columns: table => new
                {
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_Tid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenMapProd_Log", x => new { x.UUID, x.FK_Tid });
                    table.ForeignKey(
                        name: "FK_TokenMapProd_Log_Prod_Logs_FK_Tid",
                        column: x => x.FK_Tid,
                        principalTable: "Prod_Logs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TokenMapProd_Log_Tokens_UUID",
                        column: x => x.UUID,
                        principalTable: "Tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TokenMapShoppingCarts",
                columns: table => new
                {
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_Tid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenMapShoppingCarts", x => new { x.UUID, x.FK_Tid });
                    table.ForeignKey(
                        name: "FK_TokenMapShoppingCarts_ShoppingCarts_FK_Tid",
                        column: x => x.FK_Tid,
                        principalTable: "ShoppingCarts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TokenMapShoppingCarts_Tokens_UUID",
                        column: x => x.UUID,
                        principalTable: "Tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokenMapAdvertise_Log_FK_Tid",
                table: "TokenMapAdvertise_Log",
                column: "FK_Tid");

            migrationBuilder.CreateIndex(
                name: "IX_TokenMapProd_Log_FK_Tid",
                table: "TokenMapProd_Log",
                column: "FK_Tid");

            migrationBuilder.CreateIndex(
                name: "IX_TokenMapShoppingCarts_FK_Tid",
                table: "TokenMapShoppingCarts",
                column: "FK_Tid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenMapAdvertise_Log");

            migrationBuilder.DropTable(
                name: "TokenMapProd_Log");

            migrationBuilder.DropTable(
                name: "TokenMapShoppingCarts");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_FK_Tid",
                table: "ShoppingCarts",
                column: "FK_Tid");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Logs_FK_Tid",
                table: "Prod_Logs",
                column: "FK_Tid");

            migrationBuilder.CreateIndex(
                name: "IX_Advertise_Logs_FK_Tid",
                table: "Advertise_Logs",
                column: "FK_Tid");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertise_Logs_Tokens_FK_Tid",
                table: "Advertise_Logs",
                column: "FK_Tid",
                principalTable: "Tokens",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_Logs_Tokens_FK_Tid",
                table: "Prod_Logs",
                column: "FK_Tid",
                principalTable: "Tokens",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_Tokens_FK_Tid",
                table: "ShoppingCarts",
                column: "FK_Tid",
                principalTable: "Tokens",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
