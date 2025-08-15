using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowerSellingWebsite.Migrations
{
    /// <inheritdoc />
    public partial class FixNavigationFlowerCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FlowerCategoriesId",
                table: "FlowerPricing",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlowerPricing_FlowerCategoriesId",
                table: "FlowerPricing",
                column: "FlowerCategoriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_FlowerPricing_FlowerCategories_FlowerCategoriesId",
                table: "FlowerPricing",
                column: "FlowerCategoriesId",
                principalTable: "FlowerCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlowerPricing_FlowerCategories_FlowerCategoriesId",
                table: "FlowerPricing");

            migrationBuilder.DropIndex(
                name: "IX_FlowerPricing_FlowerCategoriesId",
                table: "FlowerPricing");

            migrationBuilder.DropColumn(
                name: "FlowerCategoriesId",
                table: "FlowerPricing");
        }
    }
}
