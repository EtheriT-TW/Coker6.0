using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Prod_Seed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Price",
                value: 30000.0);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "Description", "Discount", "Introduction", "Title" },
                values: new object[] { "奈米單體馬桶 W384 x D685 x H470mm\n直熱式微電腦馬桶座\n噴嘴紫外線殺菌\n獨立水壓系統\n腳觸設計\nEasy Touch開閉蓋技術\n第二代微波感應技術", 28000.0, "從座圈到噴嘴給您雙重防護\n不用動手全自動科技最體貼\n雙漩洗技術為您實現真乾淨", "DE-R1073 德瑞克直熱式微電腦馬桶座／遙控型" });

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "Description", "Introduction", "Title" },
                values: new object[] { "商品二的第一行說明\n商品二的第二行說明", "商品二的第一行介紹\n商品二的第二行介紹", "C659NA 德瑞克Smart III淨未來智慧馬桶" });

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "Description", "Introduction", "Title" },
                values: new object[] { "商品三的第一行說明\n商品二的第二行說明", "商品三的第一行介紹\n商品二的第二行介紹", "L602 檯上三角盆" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Price",
                value: 28000.0);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "Description", "Discount", "Introduction", "Title" },
                values: new object[] { "商品一的說明", null, "商品一的介紹", "商品一的名稱" });

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "Description", "Introduction", "Title" },
                values: new object[] { "商品二的說明", "商品二的介紹", "商品二的名稱" });

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "Description", "Introduction", "Title" },
                values: new object[] { "商品三的說明", "商品三的介紹", "商品三的名稱" });
        }
    }
}
