using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class VNPayConfig : BaseEntity
    {
        [Required]
        public string ConfigKey { get; set; } = null!; // e.g., "Sandbox", "Production"

        [Required]
        public string TmnCode { get; set; } = null!; // VNPay merchant code

        [Required]
        public string HashSecret { get; set; } = null!; // VNPay hash secret

        [Required]
        public string PaymentUrl { get; set; } = null!; // VNPay payment gateway URL

        [Required]
        public string ReturnUrl { get; set; } = null!; // Your return URL

        [Required]
        public string CancelUrl { get; set; } = null!; // Your cancel URL

        [Required]
        public string Locale { get; set; } = "vn"; // Default language

        [Required]
        public string CurrencyCode { get; set; } = "VND"; // Default currency

        [Required]
        public bool IsActive { get; set; } = true;

        public string? Description { get; set; }
    }
}