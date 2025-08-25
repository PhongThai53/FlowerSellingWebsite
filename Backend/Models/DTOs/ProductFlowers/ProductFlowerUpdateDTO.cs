using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs.ProductFlowers
{
    public class ProductFlowerUpdateDTO
    {
        [Required(ErrorMessage = "ProductId is required.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "FlowerId is required.")]
        public int FlowerId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }
    }
}
