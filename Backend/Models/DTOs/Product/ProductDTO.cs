using FlowerSellingWebsite.Models.DTOs.ProductPhoto;

namespace FlowerSellingWebsite.Models.DTOs.Product
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public List<ProductPhotoDTO> Photos { get; set; } = new();
    }
}
