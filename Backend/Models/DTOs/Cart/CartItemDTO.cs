using System.Text.Json.Serialization;

namespace FlowerSellingWebsite.Models.DTOs.Cart
{
    public class CartItemDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("cart_id")]
        public int CartId { get; set; }

        [JsonPropertyName("product_id")]
        public int ProductId { get; set; }

        [JsonPropertyName("product_name")]
        public string ProductName { get; set; } = null!;

        [JsonPropertyName("product_url")]
        public string ProductUrl { get; set; } = null!;

        [JsonPropertyName("product_image")]
        public string? ProductImage { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("line_total")]
        public decimal LineTotal { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}


