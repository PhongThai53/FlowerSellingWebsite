using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs.ProductFlower
{
    public class CreateProductFlowerDTO
    {
        [Required(ErrorMessage = "Flower ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Flower ID must be greater than 0")]
        public int FlowerId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int QuantityNeeded { get; set; }
    }
}

