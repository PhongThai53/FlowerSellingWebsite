using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowerSellingWebsite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IVNPayService _vnPayService;

        public PaymentController(IVNPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        [HttpGet("vnpay/return")]
        public async Task<IActionResult> VNPayReturn([FromQuery] Dictionary<string, string> queryParams)
        {
            try
            {
                // Process VNPay payment response
                var result = await _vnPayService.ProcessPaymentResponseAsync(queryParams);

                if (result.IsSuccess)
                {
                    // Payment successful - redirect to success page
                    return Redirect($"/html/Payment/payment-success.html?orderNumber={result.OrderNumber}&transactionId={result.TransactionId}&amount={result.Amount}");
                }
                else
                {
                    // Payment failed - redirect to failure page
                    return Redirect($"/html/Payment/payment-failed.html?orderNumber={result.OrderNumber}&message={result.ResponseMessage}");
                }
            }
            catch (Exception ex)
            {
                // Error occurred - redirect to error page
                return Redirect($"/payment/error?message={ex.Message}");
            }
        }

        [HttpGet("vnpay/cancel")]
        public IActionResult VNPayCancel([FromQuery] string orderNumber)
        {
            // Payment was cancelled by user
            return Redirect($"/html/Payment/payment-cancelled.html?orderNumber={orderNumber}");
        }

        [HttpGet("status/{orderNumber}")]
        public async Task<IActionResult> GetPaymentStatus(string orderNumber)
        {
            try
            {
                // This endpoint can be used to check payment status
                // You would implement the logic to get payment status from your database
                return Ok(new { orderNumber, status = "pending" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
