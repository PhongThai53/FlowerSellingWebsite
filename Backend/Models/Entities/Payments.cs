using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Payments : BaseEntity
    {
        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [Required]
        [ForeignKey("PaymentMethod")]
        public int PaymentMethodId { get; set; }

        [Required]
        public string MethodName { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public string Status { get; set; } = null!; // Pending, Completed, Failed, Refunded

        // VNPay specific fields
        public string? VNPayTransactionId { get; set; }
        public string? VNPayResponseCode { get; set; }
        public string? VNPayResponseMessage { get; set; }
        public string? VNPayBankCode { get; set; }
        public string? VNPayCardType { get; set; }
        public string? VNPaySecureHash { get; set; }
        public DateTime? VNPayProcessDate { get; set; }
        public string? VNPayGatewayUrl { get; set; }
        public string? VNPayLocale { get; set; }
        public string? VNPayCurrencyCode { get; set; }
        public string? VNPayTxnRef { get; set; }
        public string? VNPayOrderInfo { get; set; }
        public string? VNPayReturnUrl { get; set; }
        public string? VNPayCancelUrl { get; set; }

        // Navigation
        public virtual Orders Order { get; set; } = null!;
        public virtual PaymentMethods PaymentMethod { get; set; } = null!;
    }
}