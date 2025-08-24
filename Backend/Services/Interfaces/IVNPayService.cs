using FlowerSellingWebsite.Models.DTOs.Order;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IVNPayService
    {
        Task<string> CreatePaymentUrlAsync(int id, string orderNumber, decimal amount, string returnUrl, string cancelUrl, string clientIpAddress = null);
        Task<bool> ValidatePaymentResponseAsync(Dictionary<string, string> responseData);
        Task<VNPayPaymentResultDTO> ProcessPaymentResponseAsync(Dictionary<string, string> responseData);
    }

    public class VNPayPaymentResultDTO
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public string ResponseMessage { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
    }
}
