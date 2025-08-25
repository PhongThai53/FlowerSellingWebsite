using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs.FlowerColor
{
    public class FlowerColorCreateDTO
    {
        [Required]
        public string? ColorName { get; set; }
        [Required]
        public string? HexCode { get; set; }
        public string? Description { get; set; }
    }
}
