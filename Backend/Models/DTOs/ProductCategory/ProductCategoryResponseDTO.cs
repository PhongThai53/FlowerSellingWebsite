namespace FlowerSellingWebsite.Models.DTOs.ProductCategory
{
    public class ProductCategoryResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // custom field
        public int TotalProducts { get; set; }
    }
}
