namespace FlowerSellingWebsite.Models.DTOs.ProductCategory
{
    public class ProductCategoryUpdateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
