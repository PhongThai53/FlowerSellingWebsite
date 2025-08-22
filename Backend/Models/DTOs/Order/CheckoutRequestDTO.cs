using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FlowerSellingWebsite.Models.DTOs.Order
{
    public class CheckoutRequestDTO
    {
        // Customer information
        [Required]
        [JsonPropertyName("customerFirstName")]
        public string CustomerFirstName { get; set; } = null!;

        [Required]
        [JsonPropertyName("customerLastName")]
        public string CustomerLastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [JsonPropertyName("customerEmail")]
        public string CustomerEmail { get; set; } = null!;

        [Required]
        [JsonPropertyName("customerPhone")]
        public string CustomerPhone { get; set; } = null!;

        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }

        // Billing address
        [Required]
        [JsonPropertyName("country")]
        public string Country { get; set; } = null!;

        [Required]
        [JsonPropertyName("city")]
        public string City { get; set; } = null!;

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [Required]
        [JsonPropertyName("postcode")]
        public string Postcode { get; set; } = null!;

        [Required]
        [JsonPropertyName("streetAddress")]
        public string StreetAddress { get; set; } = null!;

        [JsonPropertyName("streetAddress2")]
        public string? StreetAddress2 { get; set; }

        // Order details
        [JsonPropertyName("orderNotes")]
        public string? OrderNotes { get; set; }

        // Payment and shipping
        [Required]
        [JsonPropertyName("paymentMethod")]
        public string PaymentMethod { get; set; } = null!; // "cash" or "vnpay"

        [Required]
        [JsonPropertyName("shippingMethod")]
        public string ShippingMethod { get; set; } = null!; // "flat" or "free"

        // Cart data
        [Required]
        [JsonPropertyName("cartItems")]
        public List<CheckoutCartItemDTO> CartItems { get; set; } = new();

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }
    }

    public class CheckoutCartItemDTO
    {
        [Required]
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [Required]
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
