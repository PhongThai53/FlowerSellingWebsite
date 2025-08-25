using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs.Flower
{
    public class FlowerCreateDTO
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? FlowerCategoryId { get; set; }
        [Required]
        public int FlowerTypeId { get; set; }
        [Required]
        public int FlowerColorId { get; set; }
        public string? Size { get; set; }
    }
}
