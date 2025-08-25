using System.Text.Json.Serialization;
using FlowerSellingWebsite.Models.DTOs.Product;

namespace FlowerSellingWebsite.Models.DTOs.Cart
{
    public class CartPriceCalculationDTO
    {
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("cartItems")]
        public List<CartItemPriceInfo> CartItems { get; set; } = new List<CartItemPriceInfo>();

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("serviceFee")]
        public decimal ServiceFee { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class CartItemPriceInfo
    {
        [JsonPropertyName("cartItemId")]
        public int CartItemId { get; set; }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("originalUnitPrice")]
        public decimal OriginalUnitPrice { get; set; }

        [JsonPropertyName("calculatedUnitPrice")]
        public decimal CalculatedUnitPrice { get; set; }

        [JsonPropertyName("lineTotal")]
        public decimal LineTotal { get; set; }

        [JsonPropertyName("priceDifference")]
        public decimal PriceDifference { get; set; }

        [JsonPropertyName("supplierBreakdown")]
        public List<SupplierPriceBreakdown> SupplierBreakdown { get; set; } = new List<SupplierPriceBreakdown>();
    }
}

