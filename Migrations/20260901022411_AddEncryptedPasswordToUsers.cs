using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddEncryptedPasswordToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncryptedPassword",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 1, 2, 24, 4, 16, DateTimeKind.Utc).AddTicks(9951));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 1, 2, 24, 4, 16, DateTimeKind.Utc).AddTicks(9965));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 1, 2, 24, 4, 16, DateTimeKind.Utc).AddTicks(9968));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 1, 2, 24, 4, 16, DateTimeKind.Utc).AddTicks(9972));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 1, 2, 24, 4, 16, DateTimeKind.Utc).AddTicks(9975));

            migrationBuilder.UpdateData(
                table: "Stores",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 1, 2, 24, 4, 14, DateTimeKind.Utc).AddTicks(8333));

            migrationBuilder.UpdateData(
                table: "TaxSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 1, 2, 24, 4, 17, DateTimeKind.Utc).AddTicks(5012));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedPassword",
                table: "Users");

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
    }
}
