using FlowerSellingWebsite.Models.DTOs.ProductFlowersDTO;
using FlowerSellingWebsite.Models.DTOs.ProductPhoto;

namespace FlowerSellingWebsite.Models.DTOs.Product
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Url { get; set; }
        public int stock { get; set; } = 0;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<ProductPhotoDTO?> ProductPhotos { get; set; } = new();

        public List<ProductFlowerResponseDTO?> ProductFlowers { get; set; } = new();
    }
}
