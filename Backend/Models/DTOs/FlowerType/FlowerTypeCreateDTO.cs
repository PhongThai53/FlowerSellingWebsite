using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs.FlowerType
{
    public class FlowerTypeCreateDTO
    {
        [Required]
        public string? TypeName { get; set; }
        public string? Description { get; set; }
    }
}
