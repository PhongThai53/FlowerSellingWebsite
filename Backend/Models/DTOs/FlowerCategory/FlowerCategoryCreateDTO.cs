using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs.Flower
{
    public class FlowerCategoryCreateDTO
    {
        [Required]
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }
}
