using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddProductExpirationDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "Products",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 20, 10, 51, 45, 502, DateTimeKind.Utc).AddTicks(1007));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 20, 10, 51, 45, 502, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 20, 10, 51, 45, 502, DateTimeKind.Utc).AddTicks(1016));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 20, 10, 51, 45, 502, DateTimeKind.Utc).AddTicks(1018));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 20, 10, 51, 45, 502, DateTimeKind.Utc).AddTicks(1019));

            migrationBuilder.UpdateData(
                table: "Stores",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 20, 10, 51, 45, 501, DateTimeKind.Utc).AddTicks(610));

            migrationBuilder.UpdateData(
                table: "TaxSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 20, 10, 51, 45, 502, DateTimeKind.Utc).AddTicks(3882));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 0, 50, 14, 437, DateTimeKind.Utc).AddTicks(7651));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 0, 50, 14, 437, DateTimeKind.Utc).AddTicks(7656));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 0, 50, 14, 437, DateTimeKind.Utc).AddTicks(7659));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 0, 50, 14, 437, DateTimeKind.Utc).AddTicks(7660));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 0, 50, 14, 437, DateTimeKind.Utc).AddTicks(7662));

            migrationBuilder.UpdateData(
                table: "Stores",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 0, 50, 14, 436, DateTimeKind.Utc).AddTicks(6623));

            migrationBuilder.UpdateData(
                table: "TaxSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 17, 0, 50, 14, 438, DateTimeKind.Utc).AddTicks(504));
        }
    }
}
