using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_PaymentType_MaxMinAmount_Icon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icons",
                table: "PaymentTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxAmount",
                table: "PaymentTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinAmount",
                table: "PaymentTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "Icons", "MinAmount" },
                values: new object[] { "", 1 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 199999, 30 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 49999, 1 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 199999, 1 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 5L,
                columns: new[] { "Icons", "MinAmount" },
                values: new object[] { "", 1 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 6L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 49999, 1 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 7L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 20000, 65 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 8L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 20000, 65 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 9L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 20000, 65 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 10L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 20000, 65 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 11L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 199999, 30 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 12L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 199999, 30 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 13L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 199999, 30 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 14L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "LINEPAY.png", 50000, 1 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 15L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 20000, 25 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 16L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 199999, 6 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 17L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 199999, 6 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 18L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 199999, 6 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 19L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 199999, 6 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 20L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 199999, 6 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 21L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 49999, 17 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 22L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 20000, 16 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 23L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 20000, 31 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 24L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 20000, 31 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 25L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 20000, 31 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 26L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 20000, 31 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 27L,
                columns: new[] { "Icons", "MaxAmount", "MinAmount" },
                values: new object[] { "", 199999, 6 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icons",
                table: "PaymentTypes");

            migrationBuilder.DropColumn(
                name: "MaxAmount",
                table: "PaymentTypes");

            migrationBuilder.DropColumn(
                name: "MinAmount",
                table: "PaymentTypes");
        }
    }
}
