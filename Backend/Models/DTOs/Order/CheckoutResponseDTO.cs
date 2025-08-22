using System.Text.Json.Serialization;

namespace FlowerSellingWebsite.Models.DTOs.Order
{
    public class CheckoutResponseDTO
    {
        [JsonPropertyName("succeeded")]
        public bool Succeeded { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public CheckoutResponseDataDTO? Data { get; set; }
    }

    public class CheckoutResponseDataDTO
    {
        [JsonPropertyName("orderId")]
        public int OrderId { get; set; }

        [JsonPropertyName("orderNumber")]
        public string OrderNumber { get; set; } = string.Empty;

        [JsonPropertyName("paymentUrl")]
        public string? PaymentUrl { get; set; } // For VNPay redirects

        [JsonPropertyName("paymentStatus")]
        public string PaymentStatus { get; set; } = string.Empty;

        [JsonPropertyName("orderStatus")]
        public string OrderStatus { get; set; } = string.Empty;

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("redirectUrl")]
        public string? RedirectUrl { get; set; } // For order confirmation page
    }
}
