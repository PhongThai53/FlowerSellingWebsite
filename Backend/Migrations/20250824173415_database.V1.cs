using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowerSellingWebsite.Migrations
{
    /// <inheritdoc />
    public partial class databaseV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cart_Orders_OrderId",
                table: "Cart");

            migrationBuilder.DropIndex(
                name: "IX_Cart_OrderId",
                table: "Cart");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Cart");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "Cart",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cart_OrderId",
                table: "Cart",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cart_Orders_OrderId",
                table: "Cart",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
