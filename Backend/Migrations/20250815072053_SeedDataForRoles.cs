using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FlowerSellingWebsite.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataForRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "Description", "IsDeleted", "PublicId", "RoleName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 15, 14, 22, 51, 987, DateTimeKind.Unspecified).AddTicks(6543), null, "System Admin", false, new Guid("11111111-1111-1111-1111-111111111111"), "Admin", null },
                    { 2, new DateTime(2025, 8, 15, 14, 22, 51, 987, DateTimeKind.Unspecified).AddTicks(6543), null, "System Users", false, new Guid("22222222-2222-2222-2222-222222222222"), "Users", null },
                    { 3, new DateTime(2025, 8, 15, 14, 22, 51, 987, DateTimeKind.Unspecified).AddTicks(6543), null, "System Staff", false, new Guid("33333333-3333-3333-3333-333333333333"), "Staff", null },
                    { 4, new DateTime(2025, 8, 15, 14, 22, 51, 987, DateTimeKind.Unspecified).AddTicks(6543), null, "System Supplier", false, new Guid("44444444-4444-4444-4444-444444444444"), "Supplier", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
