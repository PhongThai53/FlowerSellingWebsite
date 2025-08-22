using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class VNPayService : IVNPayService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly string _tmnCode;
        private readonly string _hashSecret;
        private readonly string _paymentUrl;
        private readonly string _returnUrl;
        private readonly string _cancelUrl;

        public VNPayService(IOrderRepository orderRepository, IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            
            // Get VNPay configuration from appsettings.json
            _tmnCode = configuration["VNPay:TmnCode"] ?? "2QXUI4J4";
            _hashSecret = configuration["VNPay:HashSecret"] ?? "KARBULZY";
            _paymentUrl = configuration["VNPay:PaymentUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            _returnUrl = configuration["VNPay:ReturnUrl"] ?? "https://localhost:3000/payment/return";
            _cancelUrl = configuration["VNPay:CancelUrl"] ?? "https://localhost:3000/payment/cancel";
        }

        public async Task<string> CreatePaymentUrlAsync(string orderNumber, decimal amount, string returnUrl, string cancelUrl)
        {
            var vnpRequest = new Dictionary<string, string>
            {
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "pay",
                ["vnp_TmnCode"] = _tmnCode,
                ["vnp_Amount"] = ((long)(amount * 100)).ToString(), // Convert to VND (smallest currency unit)
                ["vnp_CurrCode"] = "VND",
                ["vnp_TxnRef"] = orderNumber,
                ["vnp_OrderInfo"] = $"Payment for order {orderNumber}",
                ["vnp_OrderType"] = "other",
                ["vnp_Locale"] = "vn",
                ["vnp_ReturnUrl"] = returnUrl,
                ["vnp_CancelUrl"] = cancelUrl,
                ["vnp_IpAddr"] = "127.0.0.1", // This should be the actual client IP
                ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss")
            };

            // Sort parameters alphabetically
            var sortedParams = vnpRequest.OrderBy(x => x.Key).ToList();

            // Build query string
            var queryString = string.Join("&", sortedParams.Select(x => $"{x.Key}={x.Value}"));

            // Create hash
            var hashData = $"{queryString}&{_hashSecret}";
            var hash = CreateMD5Hash(hashData);

            // Add hash to parameters
            vnpRequest["vnp_SecureHash"] = hash;

            // Build final URL
            var finalQueryString = string.Join("&", vnpRequest.OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"));
            return $"{_paymentUrl}?{finalQueryString}";
        }

        public async Task<bool> ValidatePaymentResponseAsync(Dictionary<string, string> responseData)
        {
            if (!responseData.ContainsKey("vnp_SecureHash"))
                return false;

            var receivedHash = responseData["vnp_SecureHash"];
            responseData.Remove("vnp_SecureHash");

            // Sort parameters alphabetically
            var sortedParams = responseData.OrderBy(x => x.Key).ToList();

            // Build query string
            var queryString = string.Join("&", sortedParams.Select(x => $"{x.Key}={x.Value}"));

            // Create hash
            var hashData = $"{queryString}&{_hashSecret}";
            var calculatedHash = CreateMD5Hash(hashData);

            return calculatedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<VNPayPaymentResultDTO> ProcessPaymentResponseAsync(Dictionary<string, string> responseData)
        {
            var result = new VNPayPaymentResultDTO();

            if (!await ValidatePaymentResponseAsync(responseData))
            {
                result.IsSuccess = false;
                result.ResponseMessage = "Invalid payment response";
                return result;
            }

            // Extract response data
            result.TransactionId = responseData.GetValueOrDefault("vnp_TransactionNo", "");
            result.ResponseCode = responseData.GetValueOrDefault("vnp_ResponseCode", "");
            result.ResponseMessage = responseData.GetValueOrDefault("vnp_Message", "");
            result.OrderNumber = responseData.GetValueOrDefault("vnp_TxnRef", "");

            // Parse amount (VNPay returns amount in smallest currency unit)
            if (decimal.TryParse(responseData.GetValueOrDefault("vnp_Amount", "0"), out decimal amount))
            {
                result.Amount = amount / 100; // Convert back to VND
            }

            // Check if payment was successful
            result.IsSuccess = result.ResponseCode == "00";

            // Update order status if payment was successful
            if (result.IsSuccess && !string.IsNullOrEmpty(result.OrderNumber))
            {
                var order = await _orderRepository.GetOrderByOrderNumberAsync(result.OrderNumber);
                if (order != null)
                {
                    await _orderRepository.UpdatePaymentStatusAsync(order.Id, "Paid");
                    await _orderRepository.UpdateOrderStatusAsync(order.Id, "Processing");
                }
            }

            return result;
        }

        private string CreateMD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return string.Join("", hashBytes.Select(b => b.ToString("x2")));
            }
        }
    }
}
