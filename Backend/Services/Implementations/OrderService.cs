//using AutoMapper;
//using FlowerSellingWebsite.Models.DTOs;
//using FlowerSellingWebsite.Models.DTOs.Order;
//using FlowerSellingWebsite.Models.Entities;
//using FlowerSellingWebsite.Repositories.Interfaces;
//using FlowerSellingWebsite.Services.Interfaces;
//using Microsoft.EntityFrameworkCore;

//namespace FlowerSellingWebsite.Services.Implementations
//{
//    public class OrderService : IOrderService
//    {
//        private readonly IOrderRepository _orderRepository;
//        private readonly ICartRepository _cartRepository;
//        private readonly IProductRepository _productRepository;
//        private readonly IUserRepository _userRepository;
//        private readonly IVNPayService _vnPayService;
//        private readonly IConfiguration _configuration;

//        public OrderService(
//            IOrderRepository orderRepository,
//            ICartRepository cartRepository,
//            IProductRepository productRepository,
//            IUserRepository userRepository,
//            IVNPayService vnPayService,
//            IConfiguration configuration)
//        {
//            _orderRepository = orderRepository;
//            _cartRepository = cartRepository;
//            _productRepository = productRepository;
//            _userRepository = userRepository;
//            _vnPayService = vnPayService;
//            _configuration = configuration;
//        }

//        public async Task<PagedResult<OrderDTO>> GetOrderHistoryAsync(UrlQueryParams urlQueryParams, int? customerId = null)
//        {
//            // Implementation for getting order history
//            // This would need to be implemented based on your existing logic
//            throw new NotImplementedException("GetOrderHistoryAsync not implemented yet");
//        }

//        public async Task<OrderDTO?> GetOrderByIdAsync(int orderId)
//        {
//            // Implementation for getting order by ID
//            // This would need to be implemented based on your existing logic
//            throw new NotImplementedException("GetOrderByIdAsync not implemented yet");
//        }

//        public async Task<UserDTO?> GetUserByPublicIdAsync(Guid publicId)
//        {
//            try
//            {
//                var user = await _userRepository.GetByPublicIdAsync(publicId);
//                if (user == null) return null;

//                return new UserDTO
//                {
//                    Id = user.Id,
//                    PublicId = user.PublicId,
//                    UserName = user.UserName,
//                    Email = user.Email,
//                    FullName = user.FullName,
//                    RoleName = user.Role?.RoleName
//                };
//            }
//            catch
//            {
//                return null;
//            }
//        }

//        public async Task<CheckoutResponseDTO> ProcessCheckoutAsync(CheckoutRequestDTO checkoutRequest, int customerId, string clientIpAddress = null)
//        {
//            try
//            {
//                // Validate customer exists
//                var customer = await _userRepository.GetByIdAsync(customerId);
//                if (customer == null)
//                {
//                    return new CheckoutResponseDTO
//                    {
//                        Succeeded = false,
//                        Message = "Customer not found"
//                    };
//                }

//                // Validate cart items and stock
//                var validationResult = await ValidateCartItemsAsync(checkoutRequest.CartItems);
//                if (!validationResult.IsValid)
//                {
//                    return new CheckoutResponseDTO
//                    {
//                        Succeeded = false,
//                        Message = validationResult.ErrorMessage
//                    };
//                }

//                // Generate order number
//                // Generate order number
//                var orderNumber = await _orderRepository.GenerateOrderNumberAsync();

//                // Create order (for both COD and VNPay, but VNPay will be updated when payment succeeds)
//                var order = new Orders
//                {
//                    OrderNumber = orderNumber,
//                    CustomerId = customerId,
//                    OrderDate = DateTime.UtcNow,
//                    Status = "Created",
//                    Subtotal = checkoutRequest.Subtotal,
//                    DiscountAmount = 0, // No discount for now
//                    TaxAmount = 0, // No tax
//                    ShippingFee = 0, // No shipping fee
//                    TotalAmount = checkoutRequest.Subtotal, // Total equals subtotal
//                    PaymentStatus = checkoutRequest.PaymentMethod == "vnpay" ? "Pending" : "Pending",
//                    ShippingAddress = $"{checkoutRequest.StreetAddress}, {checkoutRequest.City}, {checkoutRequest.Country}",
//                    BillingAddress = $"{checkoutRequest.StreetAddress}, {checkoutRequest.City}, {checkoutRequest.Country}",
//                    Notes = checkoutRequest.OrderNotes,
//                    CustomerFirstName = checkoutRequest.CustomerFirstName,
//                    CustomerLastName = checkoutRequest.CustomerLastName,
//                    CustomerEmail = checkoutRequest.CustomerEmail,
//                    CustomerPhone = checkoutRequest.CustomerPhone,
//                    CompanyName = checkoutRequest.CompanyName,
//                    Country = checkoutRequest.Country,
//                    City = checkoutRequest.City,
//                    State = checkoutRequest.State,
//                    Postcode = checkoutRequest.Postcode,
//                    StreetAddress = checkoutRequest.StreetAddress,
//                    StreetAddress2 = checkoutRequest.StreetAddress2,
//                    CreatedBy = customer.Email,
//                    CreatedAt = DateTime.UtcNow
//                };

//                // Save order
//                var savedOrder = await _orderRepository.CreateOrderAsync(order);

//                // Create order details
//                foreach (var cartItem in checkoutRequest.CartItems)
//                {
//                    // Get product name from the cart item
//                    var productName = cartItem.ProductName ?? $"Product {cartItem.ProductId}";

//                    var orderDetail = new OrderDetails
//                    {
//                        OrderId = savedOrder.Id,
//                        ProductId = cartItem.ProductId,
//                        ItemName = productName,
//                        Quantity = cartItem.Quantity,
//                        UnitPrice = cartItem.UnitPrice,
//                        LineTotal = cartItem.LineTotal,
//                        CreatedAt = DateTime.UtcNow
//                    };

//                    await _orderRepository.CreateOrderDetailAsync(orderDetail);
//                }

//                // Create payment record
//                var paymentMethod = await GetOrCreatePaymentMethodAsync(checkoutRequest.PaymentMethod);
//                var payment = new Payments
//                {
//                    OrderId = savedOrder.Id,
//                    PaymentMethodId = paymentMethod.Id,
//                    MethodName = paymentMethod.MethodName,
//                    Description = $"Payment for order {orderNumber}",
//                    Amount = checkoutRequest.TotalAmount,
//                    PaymentDate = DateTime.UtcNow,
//                    Status = "Pending",
//                    CreatedAt = DateTime.UtcNow
//                };

//                await _orderRepository.CreatePaymentAsync(payment);

//                // For COD orders: reduce stock immediately
//                // For VNPay orders: reduce stock only after payment confirmation
//                if (checkoutRequest.PaymentMethod == "cash")
//                {
//                    var orderItems = checkoutRequest.CartItems.Select(item => (item.ProductId, item.Quantity)).ToList();

//                    // Reduce stock for each product in the order
//                    foreach (var (productId, quantity) in orderItems)
//                    {
//                        var stockReductionSuccess = await _productRepository.ReduceProductStockAsync(productId, quantity);
//                        if (!stockReductionSuccess)
//                        {
//                            // If stock reduction fails, rollback the order
//                            await _orderRepository.DeleteOrderAsync(order.Id);
//                            return new CheckoutResponseDTO
//                            {
//                                Succeeded = false,
//                                Message = $"Failed to update stock for product {productId}. Please try again."
//                            };
//                        }
//                    }

//                    // Log successful stock reduction for COD
//                    Console.WriteLine($"Successfully reduced stock for COD order {orderNumber}: {orderItems.Count} products updated");
//                }
//                else
//                {
//                    // For VNPay orders, stock will be reduced after payment confirmation
//                    Console.WriteLine($"VNPay order {orderNumber} created. Stock will be reduced after payment confirmation.");
//                }

//                // Clear cart after successful order
//                await _cartRepository.ClearCartByUserIdAsync(customerId);

//                // Prepare response
//                var response = new CheckoutResponseDTO
//                {
//                    Succeeded = true,
//                    Message = "Order created successfully",
//                    Data = new CheckoutResponseDataDTO
//                    {
//                        OrderId = savedOrder.Id,
//                        OrderNumber = orderNumber,
//                        PaymentStatus = payment.Status,
//                        OrderStatus = order.Status,
//                        TotalAmount = order.TotalAmount
//                    }
//                };

//                // Handle VNPay payment
//                if (checkoutRequest.PaymentMethod == "vnpay")
//                {
//                    try
//                    {
//                        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7062";
//                        var returnUrl = $"{baseUrl}/api/payment/vnpay/return";
//                        var cancelUrl = $"{baseUrl}/api/payment/vnpay/cancel?orderNumber={orderNumber}";

//                        // Use provided client IP or get a fallback IP
//                        var ipAddress = clientIpAddress ?? VNPayService.GetClientIpAddress();

//                        var paymentUrl = await _vnPayService.CreatePaymentUrlAsync(
//                            savedOrder.Id, // Use the actual order ID
//                            orderNumber, 
//                            checkoutRequest.TotalAmount, 
//                            returnUrl, 
//                            cancelUrl,
//                            ipAddress
//                        );

//                        if (string.IsNullOrEmpty(paymentUrl))
//                        {
//                            throw new InvalidOperationException("VNPay payment URL is empty");
//                        }

//                        // Test if the VNPay URL is actually valid by checking if it contains required parameters
//                        if (!paymentUrl.Contains("vnp_SecureHash=") || !paymentUrl.Contains("vnp_TmnCode="))
//                        {
//                            throw new InvalidOperationException("VNPay payment URL is malformed");
//                        }

//                        response.Data.PaymentUrl = paymentUrl;
//                        response.Message = "Order created successfully. Redirecting to VNPay for payment...";
//                    }
//                    catch (Exception ex)
//                    {
//                        // If VNPay fails, ROLLBACK the order and return error
//                        await _orderRepository.DeleteOrderAsync(order.Id);

//                        response.Succeeded = false;
//                        response.Message = $"VNPay payment setup failed: {ex.Message}";
//                        response.Data = null;

//                        return response;
//                    }
//                }
//                else
//                {
//                    // For COD, redirect to order confirmation
//                    response.Data.RedirectUrl = $"/html/user/order-confirmation.html?orderNumber={orderNumber}";
//                }

//                return response;
//            }
//            catch (Exception ex)
//            {
//                return new CheckoutResponseDTO
//                {
//                    Succeeded = false,
//                    Message = $"Error processing checkout: {ex.Message}"
//                };
//            }
//        }

//        private async Task<(bool IsValid, string ErrorMessage)> ValidateCartItemsAsync(List<CheckoutCartItemDTO> cartItems)
//        {
//            foreach (var item in cartItems)
//            {
//                var product = await _productRepository.GetProductByIdAsync(item.ProductId);
//                if (product == null)
//                {
//                    return (false, $"Product with ID {item.ProductId} not found");
//                }

//                // Check stock availability
//                if (product.Stock < item.Quantity)
//                {
//                    return (false, $"Insufficient stock for product {product.Name}. Available: {product.Stock}, Requested: {item.Quantity}");
//                }
//            }

//            return (true, string.Empty);
//        }

//        private async Task<PaymentMethods> GetOrCreatePaymentMethodAsync(string paymentMethodName)
//        {
//            try
//            {
//                // First, try to find existing payment method
//                var existingMethod = await _orderRepository.GetPaymentMethodByNameAsync(paymentMethodName);
//                if (existingMethod != null)
//                {
//                    return existingMethod;
//                }

//                // If not found, create a new one
//                var methodName = paymentMethodName switch
//                {
//                    "cash" => "Cash on Delivery",
//                    "vnpay" => "VNPay",
//                    _ => "Unknown"
//                };

//                var newPaymentMethod = new PaymentMethods
//                {
//                    MethodName = methodName,
//                    MethodType = paymentMethodName == "vnpay" ? "VNPay" : "Standard",
//                    IsActive = true,
//                    DisplayName = methodName,
//                    CreatedAt = DateTime.UtcNow
//                };

//                // Save the new payment method to database
//                var savedMethod = await _orderRepository.CreatePaymentMethodAsync(newPaymentMethod);
//                return savedMethod;
//            }
//            catch (Exception ex)
//            {
//                // If payment method creation fails, return a default one
//                // This is a fallback to prevent order creation from failing
//                return new PaymentMethods
//                {
//                    Id = 1, // Use a default ID
//                    MethodName = "Cash on Delivery",
//                    MethodType = "Standard",
//                    IsActive = true,
//                    DisplayName = "Cash on Delivery",
//                    CreatedAt = DateTime.UtcNow
//                };
//            }
//        }

//        public async Task<CheckoutResponseDTO> ConfirmCODOrderAsync(int orderId, int customerId)
//        {
//            try
//            {
//                // Get the order
//                var order = await _orderRepository.GetOrderByIdAsync(orderId);
//                if (order == null)
//                {
//                    return new CheckoutResponseDTO
//                    {
//                        Succeeded = false,
//                        Message = "Order not found"
//                    };
//                }

//                // Verify the order belongs to the customer
//                if (order.CustomerId != customerId)
//                {
//                    return new CheckoutResponseDTO
//                    {
//                        Succeeded = false,
//                        Message = "Order does not belong to this customer"
//                    };
//                }

//                // Check if order is already confirmed
//                if (order.Status == "Confirmed" || order.PaymentStatus == "Paid")
//                {
//                    return new CheckoutResponseDTO
//                    {
//                        Succeeded = false,
//                        Message = "Order is already confirmed"
//                    };
//                }

//                // Get order details to reduce stock
//                var orderDetails = await _orderRepository.GetOrderDetailsByOrderIdAsync(orderId);
//                if (orderDetails == null || !orderDetails.Any())
//                {
//                    return new CheckoutResponseDTO
//                    {
//                        Succeeded = false,
//                        Message = "No order details found"
//                    };
//                }

//                // Reduce stock for all products in the order
//                foreach (var detail in orderDetails)
//                {
//                    var stockReductionSuccess = await _productRepository.ReduceProductStockAsync(detail.ProductId, detail.Quantity);
//                    if (!stockReductionSuccess)
//                    {
//                        return new CheckoutResponseDTO
//                        {
//                            Succeeded = false,
//                            Message = $"Failed to update stock for product {detail.ProductId}. Please try again."
//                        };
//                    }
//                }

//                // Update order status to confirmed
//                var statusUpdated = await _orderRepository.UpdateOrderStatusAsync(orderId, "Confirmed");
//                var paymentStatusUpdated = await _orderRepository.UpdatePaymentStatusAsync(orderId, "Paid");

//                if (!statusUpdated || !paymentStatusUpdated)
//                {
//                    return new CheckoutResponseDTO
//                    {
//                        Succeeded = false,
//                        Message = "Failed to update order status"
//                    };
//                }

//                // Log successful confirmation
//                Console.WriteLine($"COD order {orderId} confirmed successfully. Stock reduced for {orderDetails.Count()} products.");

//                return new CheckoutResponseDTO
//                {
//                    Succeeded = true,
//                    Message = "Order confirmed successfully",
//                    Data = new CheckoutResponseDataDTO
//                    {
//                        OrderId = orderId,
//                        OrderNumber = order.OrderNumber,
//                        PaymentStatus = "Paid",
//                        OrderStatus = "Confirmed",
//                        TotalAmount = order.TotalAmount
//                    }
//                };
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error confirming COD order {orderId}: {ex.Message}");
//                return new CheckoutResponseDTO
//                {
//                    Succeeded = false,
//                    Message = $"Error confirming order: {ex.Message}"
//                };
//            }
//        }
//    }
//}
