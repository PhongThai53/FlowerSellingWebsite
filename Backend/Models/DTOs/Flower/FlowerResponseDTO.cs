using FlowerSellingWebsite.Models.DTOs.FlowerCategory;
using FlowerSellingWebsite.Models.DTOs.FlowerColor;
using FlowerSellingWebsite.Models.DTOs.FlowerType;

namespace FlowerSellingWebsite.Models.DTOs.Flower
{
    public class FlowerResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? FlowerCategoryId { get; set; }
        public int FlowerTypeId { get; set; }
        public int FlowerColorId { get; set; }
        public string? Size { get; set; }

        // Navigation (để nguyên)
        public FlowerCategoryResponseDTO? FlowerCategory { get; set; }
        public FlowerTypeResponseDTO? FlowerType { get; set; }
        public FlowerColorResponseDTO? FlowerColor { get; set; }

        // Nếu muốn bạn có thể thêm collection navigation DTOs ở đây
    }
}
