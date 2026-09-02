using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 27, 3, 37, 10, 895, DateTimeKind.Utc).AddTicks(7309));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 27, 3, 37, 10, 895, DateTimeKind.Utc).AddTicks(7320));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 27, 3, 37, 10, 895, DateTimeKind.Utc).AddTicks(7323));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 27, 3, 37, 10, 895, DateTimeKind.Utc).AddTicks(7325));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 27, 3, 37, 10, 895, DateTimeKind.Utc).AddTicks(7327));

            migrationBuilder.UpdateData(
                table: "Stores",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 27, 3, 37, 10, 894, DateTimeKind.Utc).AddTicks(6234));

            migrationBuilder.UpdateData(
                table: "TaxSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 27, 3, 37, 10, 896, DateTimeKind.Utc).AddTicks(1112));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
