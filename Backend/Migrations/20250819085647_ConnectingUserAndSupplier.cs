using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowerSellingWebsite.Migrations
{
    /// <inheritdoc />
    public partial class ConnectingUserAndSupplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SupplierId",
                table: "Users",
                column: "SupplierId",
                unique: true,
                filter: "[SupplierId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Suppliers_SupplierId",
                table: "Users",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Suppliers_SupplierId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SupplierId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Users");
        }
    }
}
