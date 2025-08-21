using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FlowerSellingWebsite.Models.DTOs.Cart
{
    public class UpdateCartItemDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}

