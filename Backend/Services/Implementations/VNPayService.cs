using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Text;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;

namespace FlowerSellingWebsite.Services.Implementations
{
	public class VNPayService : IVNPayService
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IVnpay _vnpay;
		private readonly string _tmnCode;
		private readonly string _hashSecret;
		private readonly string _baseUrl;
		private readonly string _callbackUrl;

		public VNPayService(IOrderRepository orderRepository, IVnpay vnPayService, IConfiguration configuration)
		{
			_orderRepository = orderRepository;
			_vnpay = vnPayService;

			_tmnCode = configuration["VNPay:TmnCode"] ?? configuration["Vnpay:TmnCode"] ?? throw new InvalidOperationException("VNPay:TmnCode is not configured");
			_hashSecret = configuration["VNPay:HashSecret"] ?? configuration["Vnpay:HashSecret"] ?? throw new InvalidOperationException("VNPay:HashSecret is not configured");
			_baseUrl = (configuration["VNPay:PaymentUrl"] ?? configuration["Vnpay:BaseUrl"] ?? throw new InvalidOperationException("VNPay:PaymentUrl/BaseUrl is not configured")).Trim();
			_callbackUrl = configuration["VNPay:ReturnUrl"] ?? configuration["Vnpay:CallbackUrl"] ?? throw new InvalidOperationException("VNPay:ReturnUrl/CallbackUrl is not configured");

			_vnpay.Initialize(_tmnCode, _hashSecret, _baseUrl, _callbackUrl);
		}

		public async Task<string> CreatePaymentUrlAsync(string orderNumber, decimal amount, string returnUrl, string cancelUrl, string clientIpAddress = null)
		{
			try
			{
				var ip = string.IsNullOrWhiteSpace(clientIpAddress) ? "127.0.0.1" : clientIpAddress;
				var request = new PaymentRequest
				{
					PaymentId = DateTime.UtcNow.Ticks,
					Money = (double)amount, // library handles unit conversion
					Description = $"Payment for order {orderNumber}",
					IpAddress = ip,
					BankCode = BankCode.ANY,
					CreatedDate = DateTime.Now,
					Currency = Currency.VND,
					Language = DisplayLanguage.Vietnamese,
				};

				var url = _vnpay.GetPaymentUrl(request);

				Console.WriteLine($"VNPay URL (VNPAY.NET) generated:\nOrder: {orderNumber}\nAmount: {amount}\nURL: {url}");
				return await Task.FromResult(url);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error generating VNPay URL: {ex.Message}");
				throw;
			}
		}

		public async Task<bool> ValidatePaymentResponseAsync(Dictionary<string, string> responseData)
		{
			try
			{
				if (responseData == null || responseData.Count == 0) return false;
				var dict = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
				foreach (var kv in responseData)
				{
					dict[kv.Key] = new StringValues(kv.Value);
				}
				var query = new QueryCollection(dict);
				var result = _vnpay.GetPaymentResult(query);
				return await Task.FromResult(result.IsSuccess);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error validating VNPay response: {ex.Message}");
				return false;
			}
		}

		public async Task<VNPayPaymentResultDTO> ProcessPaymentResponseAsync(Dictionary<string, string> responseData)
		{
			var resultDto = new VNPayPaymentResultDTO();
			try
			{
				var dict = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
				foreach (var kv in responseData)
				{
					dict[kv.Key] = new StringValues(kv.Value);
				}
				var query = new QueryCollection(dict);
				var paymentResult = _vnpay.GetPaymentResult(query);

				resultDto.IsSuccess = paymentResult.IsSuccess;
				resultDto.ResponseCode = paymentResult.PaymentResponse?.Code.ToString() ?? string.Empty;
				resultDto.ResponseMessage = paymentResult.PaymentResponse?.Description ?? string.Empty;
				resultDto.TransactionId = paymentResult.VnpayTransactionId.ToString();
				resultDto.OrderNumber = responseData.GetValueOrDefault("vnp_TxnRef", string.Empty);
				// Amount: VNPay returns in smallest unit via vnp_Amount
				if (decimal.TryParse(responseData.GetValueOrDefault("vnp_Amount", "0"), out var raw))
				{
					resultDto.Amount = raw / 100m;
				}

				if (resultDto.IsSuccess && !string.IsNullOrEmpty(resultDto.OrderNumber))
				{
					var order = await _orderRepository.GetOrderByOrderNumberAsync(resultDto.OrderNumber);
					if (order != null)
					{
						await _orderRepository.UpdatePaymentStatusAsync(order.Id, "Paid");
						await _orderRepository.UpdateOrderStatusAsync(order.Id, "Processing");
					}
				}
			}
			catch (Exception ex)
			{
				resultDto.IsSuccess = false;
				resultDto.ResponseMessage = ex.Message;
			}

			return resultDto;
		}

		public static string GetClientIpAddress()
		{
			return "127.0.0.1";
		}
	}
}
