using System.Text.Json.Serialization;

namespace FlowerSellingWebsite.Models.DTOs.Cart
{
    public class PagedCartResultDTO
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        [JsonPropertyName("total_items")]
        public long TotalItems { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("has_next")]
        public bool HasNext => Page < TotalPages;

        [JsonPropertyName("has_prev")]
        public bool HasPrev => Page > 1;

        [JsonPropertyName("cart_items")]
        public IReadOnlyList<CartItemDTO> CartItems { get; set; } = Array.Empty<CartItemDTO>();

        [JsonPropertyName("cart_summary")]
        public CartSummaryDTO CartSummary { get; set; } = new CartSummaryDTO();
    }

    public class CartSummaryDTO
    {
        [JsonPropertyName("total_items")]
        public int TotalItems { get; set; }

        [JsonPropertyName("total_amount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("cart_id")]
        public int CartId { get; set; }
    }
}
