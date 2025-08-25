//using FlowerSellingWebsite.Services.Interfaces;
//using FlowerSellingWebsite.Repositories.Interfaces;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.DependencyInjection;

//namespace FlowerSellingWebsite.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PaymentController : ControllerBase
//    {
//        private readonly IVNPayService _vnPayService;
//        private readonly IOrderRepository _orderRepository;

//        public PaymentController(IVNPayService vnPayService, IOrderRepository orderRepository)
//        {
//            _vnPayService = vnPayService;
//            _orderRepository = orderRepository;
//        }

//        [HttpGet("vnpay/return")]
//        public async Task<IActionResult> VNPayReturn([FromQuery] Dictionary<string, string> queryParams)
//        {
//            try
//            {
//                Console.WriteLine("=== VNPAY CALLBACK RECEIVED ===");
//                Console.WriteLine($"VNPay Return - Query Params: {string.Join(", ", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}");

//                // Check if we have the required parameters
//                if (!queryParams.ContainsKey("vnp_TxnRef") || !queryParams.ContainsKey("vnp_ResponseCode"))
//                {
//                    Console.WriteLine("Missing required VNPay parameters");
//                    return Redirect($"/html/Payment/payment-failed.html?message=Missing payment parameters");
//                }

//                Console.WriteLine($"Order ID (vnp_TxnRef): {queryParams.GetValueOrDefault("vnp_TxnRef")}");
//                Console.WriteLine($"Response Code: {queryParams.GetValueOrDefault("vnp_ResponseCode")}");

//                // Process VNPay payment response
//                Console.WriteLine("Processing VNPay response...");
//                var result = await _vnPayService.ProcessPaymentResponseAsync(queryParams);

//                Console.WriteLine($"VNPay Return - Result: Success={result.IsSuccess}, OrderNumber={result.OrderNumber}, Message={result.ResponseMessage}");

//                if (result.IsSuccess)
//                {
//                    Console.WriteLine("VNPay payment succeeded, updating order status...");

//                    // For VNPay: Update the order status now that payment has succeeded
//                    if (!string.IsNullOrEmpty(result.OrderNumber))
//                    {
//                        // VNPay returns the order ID in vnp_TxnRef, not the order number
//                        if (int.TryParse(result.OrderNumber, out int orderId))
//                        {
//                            Console.WriteLine($"Looking for order with ID: {orderId}");
//                            var existingOrder = await _orderRepository.GetOrderByIdAsync(orderId);

//                            if (existingOrder != null)
//                            {
//                                Console.WriteLine($"Order {existingOrder.Id} found, updating payment status to Paid");
//                                Console.WriteLine($"Current order status: {existingOrder.Status}");
//                                Console.WriteLine($"Current payment status: {existingOrder.PaymentStatus}");

//                                // Update Orders.PaymentStatus to "Paid"
//                                var paymentStatusUpdated = await _orderRepository.UpdatePaymentStatusAsync(existingOrder.Id, "Paid");
//                                Console.WriteLine($"Orders.PaymentStatus updated: {paymentStatusUpdated}");

//                                // Update Payments.Status to "Completed" 
//                                var paymentEntityUpdated = await _orderRepository.UpdatePaymentEntityStatusAsync(existingOrder.Id, "Paid");
//                                Console.WriteLine($"Payments.Status updated: {paymentEntityUpdated}");

//                                // Keep order status as "Created" (don't change to "Processing")
//                                Console.WriteLine($"Order status remains: {existingOrder.Status}");

//                                // Now reduce stock since payment is confirmed
//                                // Only reduce stock if it hasn't been reduced yet
//                                if (existingOrder.Status != "StockReduced")
//                                {
//                                    await ReduceStockForConfirmedOrder(existingOrder.Id);

//                                    // Mark that stock has been reduced
//                                    await _orderRepository.UpdateOrderStatusAsync(existingOrder.Id, "StockReduced");
//                                }

//                                // Verify the update
//                                var updatedOrder = await _orderRepository.GetOrderByIdAsync(orderId);
//                                if (updatedOrder != null)
//                                {
//                                    Console.WriteLine($"Updated order - Status: {updatedOrder.Status}, PaymentStatus: {updatedOrder.PaymentStatus}");
//                                }
//                            }
//                            else
//                            {
//                                Console.WriteLine($"Order not found for order ID: {orderId}");
//                            }
//                        }
//                        else
//                        {
//                            Console.WriteLine($"Could not parse order ID from: {result.OrderNumber}");
//                        }
//                    }
//                    else
//                    {
//                        Console.WriteLine("Result.OrderNumber is null or empty");
//                    }

//                    // Payment successful - redirect to success page
//                    Console.WriteLine("Redirecting to payment success page...");
//                    return Redirect($"/html/Payment/payment-success.html?orderNumber={result.OrderNumber}&transactionId={result.TransactionId}&amount={result.Amount}");
//                }
//                else
//                {
//                    Console.WriteLine("VNPay payment failed, redirecting to failure page");
//                    // Payment failed - redirect to failure page
//                    return Redirect($"/html/Payment/payment-failed.html?orderNumber={result.OrderNumber}&message={result.ResponseMessage}");
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error in VNPay return: {ex.Message}");
//                Console.WriteLine($"Stack trace: {ex.StackTrace}");
//                // Error occurred - redirect to error page
//                return Redirect($"/payment/error?message={ex.Message}");
//            }
//        }

//        [HttpGet("vnpay/cancel")]
//        public IActionResult VNPayCancel([FromQuery] string orderNumber)
//        {
//            // Payment was cancelled by user
//            return Redirect($"/html/Payment/payment-cancelled.html?orderNumber={orderNumber}");
//        }

//        [HttpGet("status/{orderNumber}")]
//        public async Task<IActionResult> GetPaymentStatus(string orderNumber)
//        {
//            try
//            {
//                // This endpoint can be used to check payment status
//                // You would implement the logic to get payment status from your database
//                return Ok(new { orderNumber, status = "pending" });
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(new { error = ex.Message });
//            }
//        }

//        private async Task ReduceStockForConfirmedOrder(int orderId)
//        {
//            try
//            {
//                // Get the order to check if stock has already been reduced
//                var order = await _orderRepository.GetOrderByIdAsync(orderId);
//                if (order == null)
//                {
//                    Console.WriteLine($"Order {orderId} not found for stock reduction");
//                    return;
//                }

//                // Check if stock has already been reduced
//                if (order.Status == "StockReduced")
//                {
//                    Console.WriteLine($"Stock already reduced for order {orderId}");
//                    return;
//                }

//                // Get order details to find products and quantities
//                var orderDetails = await _orderRepository.GetOrderDetailsByOrderIdAsync(orderId);
//                if (orderDetails == null || !orderDetails.Any())
//                {
//                    Console.WriteLine($"No order details found for order {orderId}");
//                    return;
//                }

//                // Get product repository to reduce stock
//                var productRepository = HttpContext.RequestServices.GetRequiredService<IProductRepository>();

//                bool allStockReduced = true;
//                foreach (var detail in orderDetails)
//                {
//                    var stockReductionSuccess = await productRepository.ReduceProductStockAsync(detail.ProductId, detail.Quantity);
//                    if (stockReductionSuccess)
//                    {
//                        Console.WriteLine($"Successfully reduced stock for product {detail.ProductId} by {detail.Quantity}");
//                    }
//                    else
//                    {
//                        Console.WriteLine($"Failed to reduce stock for product {detail.ProductId} by {detail.Quantity}");
//                        allStockReduced = false;
//                    }
//                }

//                if (allStockReduced)
//                {
//                    Console.WriteLine($"All stock successfully reduced for order {orderId}");
//                }
//                else
//                {
//                    Console.WriteLine($"Some stock reduction failed for order {orderId}");
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error reducing stock for confirmed order {orderId}: {ex.Message}");
//            }
//        }
//    }
//}
