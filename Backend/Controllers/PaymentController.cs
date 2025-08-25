using FlowerSellingWebsite.Services.Interfaces;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace FlowerSellingWebsite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IVNPayService _vnPayService;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;

        public PaymentController(IVNPayService vnPayService, IOrderRepository orderRepository, IOrderService orderService)
        {
            _vnPayService = vnPayService;
            _orderRepository = orderRepository;
            _orderService = orderService;
        }

        [HttpGet("vnpay/return")]
        public async Task<IActionResult> VNPayReturn([FromQuery] Dictionary<string, string> queryParams)
        {
            try
            {
                if (!queryParams.ContainsKey("vnp_TxnRef") || !queryParams.ContainsKey("vnp_ResponseCode"))
                {
                    return Redirect($"/html/Payment/payment-failed.html?message=Missing payment parameters");
                }

                var result = await _vnPayService.ProcessPaymentResponseAsync(queryParams);

                if (result.IsSuccess)
                {
                    if (int.TryParse(result.OrderNumber, out int orderId))
                    {
                        var existingOrder = await _orderRepository.GetOrderByIdAsync(orderId);
                        if (existingOrder != null)
                        {
                            await _orderRepository.UpdatePaymentStatusAsync(existingOrder.Id, "Paid");
                            await _orderRepository.UpdatePaymentEntityStatusAsync(existingOrder.Id, "Paid");
                            // Deduct flowers and create POs now that payment is confirmed
                            await _orderService.ConfirmPaidOrderAsync(orderId);
                            // Redirect to frontend order confirmation page
                            var feBase = "https://localhost:7203"; // fallback if not in config
                            var orderNumber = existingOrder.OrderNumber;
                            return Redirect($"{feBase}/html/user/order-confirmation.html?orderId={existingOrder.Id}&orderNumber={orderNumber}&paymentMethod=VNPay&status=Created");
                        }
                    }
                    // Fallback redirect if order not found
                    return Redirect($"https://localhost:7203/html/user/order-confirmation.html?orderId={result.OrderNumber}&paymentMethod=VNPay&status=Created");
                }

                return Redirect($"https://localhost:7203/html/Payment/payment-failed.html?orderNumber={result.OrderNumber}&message={result.ResponseMessage}");
            }
            catch (Exception ex)
            {
                return Redirect($"https://localhost:7203/html/Payment/payment-failed.html?message={Uri.EscapeDataString(ex.Message)}");
            }
        }

        [HttpGet("vnpay/cancel")]
        public IActionResult VNPayCancel([FromQuery] string orderNumber)
        {
            return Redirect($"/html/Payment/payment-cancelled.html?orderNumber={orderNumber}");
        }
    }
}
