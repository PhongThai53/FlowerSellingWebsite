using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs.FlowerType
{
    public interface FlowerTypeUpdateDTO
    {
        [Required]
        public string? TypeName { get; set; }
        public string? Description { get; set; }
    }
}
