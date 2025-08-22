using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace FlowerSellingWebsite.Models.DTOs.Cart
{
    public class CartDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("order_id")]
        public int? OrderId { get; set; }

        [JsonPropertyName("is_checked_out")]
        public bool IsCheckedOut { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("cart_items")]
        public List<CartItemDTO> CartItems { get; set; } = new List<CartItemDTO>();

        [JsonPropertyName("total_items")]
        public int TotalItems => CartItems?.Sum(item => item.Quantity) ?? 0;

        [JsonPropertyName("total_amount")]
        public decimal TotalAmount => CartItems?.Sum(item => item.LineTotal) ?? 0;
    }
}





