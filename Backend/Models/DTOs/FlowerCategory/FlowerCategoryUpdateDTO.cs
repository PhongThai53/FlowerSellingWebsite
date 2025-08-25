using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs.Flower
{
    public class FlowerCategoryUpdateDTO
    {
        [Required]
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }
}
