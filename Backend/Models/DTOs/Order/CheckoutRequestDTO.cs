using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FlowerSellingWebsite.Models.DTOs.Order
{
    public class CheckoutRequestDTO
    {
        // Customer information - these will be populated from user profile
        [JsonPropertyName("customerFirstName")]
        public string? CustomerFirstName { get; set; }

        [JsonPropertyName("customerLastName")]
        public string? CustomerLastName { get; set; }

        [JsonPropertyName("customerEmail")]
        public string? CustomerEmail { get; set; }

        [JsonPropertyName("customerPhone")]
        public string? CustomerPhone { get; set; }

        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }

        // Billing address
        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("postcode")]
        public string? Postcode { get; set; }

        [JsonPropertyName("streetAddress")]
        public string? StreetAddress { get; set; }

        [JsonPropertyName("streetAddress2")]
        public string? StreetAddress2 { get; set; }

        // Order details
        [JsonPropertyName("orderNotes")]
        public string? OrderNotes { get; set; }

        // Payment and shipping
        [JsonPropertyName("paymentMethod")]
        public string? PaymentMethod { get; set; } // "cash" or "vnpay"

        [JsonPropertyName("shippingMethod")]
        public string? ShippingMethod { get; set; } // "flat" or "free"

        // Cart data
        [JsonPropertyName("cartItems")]
        public List<CheckoutCartItemDTO>? CartItems { get; set; } = new();

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        // New fields for the updated flow
        [JsonPropertyName("shippingAddress")]
        public string? ShippingAddress { get; set; }

        [JsonPropertyName("billingAddress")]
        public string? BillingAddress { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("discountAmount")]
        public decimal? DiscountAmount { get; set; }

        [JsonPropertyName("shippingFee")]
        public decimal? ShippingFee { get; set; }
    }

    public class CheckoutCartItemDTO
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("lineTotal")]
        public decimal LineTotal { get; set; }

        [JsonPropertyName("productName")]
        public string? ProductName { get; set; }
    }
}
