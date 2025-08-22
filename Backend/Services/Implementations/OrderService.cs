using AutoMapper;
using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Order;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVNPayService _vnPayService;
        private readonly IConfiguration _configuration;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IVNPayService vnPayService,
            IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _vnPayService = vnPayService;
            _configuration = configuration;
        }

        public async Task<PagedResult<OrderDTO>> GetOrderHistoryAsync(UrlQueryParams urlQueryParams, int? customerId = null)
        {
            // Implementation for getting order history
            // This would need to be implemented based on your existing logic
            throw new NotImplementedException("GetOrderHistoryAsync not implemented yet");
        }

        public async Task<OrderDTO?> GetOrderByIdAsync(int orderId)
        {
            // Implementation for getting order by ID
            // This would need to be implemented based on your existing logic
            throw new NotImplementedException("GetOrderByIdAsync not implemented yet");
        }

        public async Task<UserDTO?> GetUserByPublicIdAsync(Guid publicId)
        {
            try
            {
                var user = await _userRepository.GetByPublicIdAsync(publicId);
                if (user == null) return null;

                return new UserDTO
                {
                    Id = user.Id,
                    PublicId = user.PublicId,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    RoleName = user.Role?.RoleName
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<CheckoutResponseDTO> ProcessCheckoutAsync(CheckoutRequestDTO checkoutRequest, int customerId)
        {
            try
            {
                // Validate customer exists
                var customer = await _userRepository.GetByIdAsync(customerId);
                if (customer == null)
                {
                    return new CheckoutResponseDTO
                    {
                        Succeeded = false,
                        Message = "Customer not found"
                    };
                }

                // Validate cart items and stock
                var validationResult = await ValidateCartItemsAsync(checkoutRequest.CartItems);
                if (!validationResult.IsValid)
                {
                    return new CheckoutResponseDTO
                    {
                        Succeeded = false,
                        Message = validationResult.ErrorMessage
                    };
                }

                // Generate order number
                var orderNumber = await _orderRepository.GenerateOrderNumberAsync();

                // Create order
                var order = new Orders
                {
                    OrderNumber = orderNumber,
                    CustomerId = customerId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Created",
                    Subtotal = checkoutRequest.Subtotal,
                    DiscountAmount = 0, // No discount for now
                    TaxAmount = 0, // No tax
                    ShippingFee = 0, // No shipping fee
                    TotalAmount = checkoutRequest.Subtotal, // Total equals subtotal
                    PaymentStatus = checkoutRequest.PaymentMethod == "vnpay" ? "Pending" : "Pending",
                    ShippingAddress = $"{checkoutRequest.StreetAddress}, {checkoutRequest.City}, {checkoutRequest.Country}",
                    BillingAddress = $"{checkoutRequest.StreetAddress}, {checkoutRequest.City}, {checkoutRequest.Country}",
                    Notes = checkoutRequest.OrderNotes,
                    CustomerFirstName = checkoutRequest.CustomerFirstName,
                    CustomerLastName = checkoutRequest.CustomerLastName,
                    CustomerEmail = checkoutRequest.CustomerEmail,
                    CustomerPhone = checkoutRequest.CustomerPhone,
                    CompanyName = checkoutRequest.CompanyName,
                    Country = checkoutRequest.Country,
                    City = checkoutRequest.City,
                    State = checkoutRequest.State,
                    Postcode = checkoutRequest.Postcode,
                    StreetAddress = checkoutRequest.StreetAddress,
                    StreetAddress2 = checkoutRequest.StreetAddress2,
                    CreatedBy = customer.Email,
                    CreatedAt = DateTime.UtcNow
                };

                // Save order
                var savedOrder = await _orderRepository.CreateOrderAsync(order);

                // Create order details
                foreach (var cartItem in checkoutRequest.CartItems)
                {
                    // Get product name from the cart item
                    var productName = cartItem.ProductName ?? $"Product {cartItem.ProductId}";
                    
                    var orderDetail = new OrderDetails
                    {
                        OrderId = savedOrder.Id,
                        ProductId = cartItem.ProductId,
                        ItemName = productName,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice,
                        LineTotal = cartItem.LineTotal,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _orderRepository.CreateOrderDetailAsync(orderDetail);
                }

                // Create payment record
                var paymentMethod = await GetOrCreatePaymentMethodAsync(checkoutRequest.PaymentMethod);
                var payment = new Payments
                {
                    OrderId = savedOrder.Id,
                    PaymentMethodId = paymentMethod.Id,
                    MethodName = paymentMethod.MethodName,
                    Description = $"Payment for order {orderNumber}",
                    Amount = checkoutRequest.TotalAmount,
                    PaymentDate = DateTime.UtcNow,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                await _orderRepository.CreatePaymentAsync(payment);

                // Clear cart after successful order
                await _cartRepository.ClearCartAsync(customerId);

                // Prepare response
                var response = new CheckoutResponseDTO
                {
                    Succeeded = true,
                    Message = "Order created successfully",
                    Data = new CheckoutResponseDataDTO
                    {
                        OrderId = savedOrder.Id,
                        OrderNumber = orderNumber,
                        PaymentStatus = payment.Status,
                        OrderStatus = order.Status,
                        TotalAmount = order.TotalAmount
                    }
                };

                // Handle VNPay payment
                if (checkoutRequest.PaymentMethod == "vnpay")
                {
                    try
                    {
                        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7062";
                        var returnUrl = $"{baseUrl}/api/payment/vnpay/return";
                        var cancelUrl = $"{baseUrl}/api/payment/vnpay/cancel?orderNumber={orderNumber}";
                        
                        var paymentUrl = await _vnPayService.CreatePaymentUrlAsync(
                            orderNumber, 
                            checkoutRequest.TotalAmount, 
                            returnUrl, 
                            cancelUrl
                        );
                        
                        response.Data.PaymentUrl = paymentUrl;
                    }
                    catch (Exception ex)
                    {
                        // If VNPay URL generation fails, still return success but with error message
                        response.Message = $"Order created but VNPay payment URL generation failed: {ex.Message}";
                    }
                }
                else
                {
                    // For COD, redirect to order confirmation
                    response.Data.RedirectUrl = $"/order-confirmation/{orderNumber}";
                }

                return response;
            }
            catch (Exception ex)
            {
                return new CheckoutResponseDTO
                {
                    Succeeded = false,
                    Message = $"Error processing checkout: {ex.Message}"
                };
            }
        }

        private async Task<(bool IsValid, string ErrorMessage)> ValidateCartItemsAsync(List<CheckoutCartItemDTO> cartItems)
        {
            foreach (var item in cartItems)
            {
                var product = await _productRepository.GetProductByIdAsync(item.ProductId);
                if (product == null)
                {
                    return (false, $"Product with ID {item.ProductId} not found");
                }

                // Check stock availability
                if (product.Stock < item.Quantity)
                {
                    return (false, $"Insufficient stock for product {product.Name}. Available: {product.Stock}, Requested: {item.Quantity}");
                }
            }

            return (true, string.Empty);
        }

        private async Task<PaymentMethods> GetOrCreatePaymentMethodAsync(string paymentMethodName)
        {
            try
            {
                // First, try to find existing payment method
                var existingMethod = await _orderRepository.GetPaymentMethodByNameAsync(paymentMethodName);
                if (existingMethod != null)
                {
                    return existingMethod;
                }

                // If not found, create a new one
                var methodName = paymentMethodName switch
                {
                    "cash" => "Cash on Delivery",
                    "vnpay" => "VNPay",
                    _ => "Unknown"
                };

                var newPaymentMethod = new PaymentMethods
                {
                    MethodName = methodName,
                    MethodType = paymentMethodName == "vnpay" ? "VNPay" : "Standard",
                    IsActive = true,
                    DisplayName = methodName,
                    CreatedAt = DateTime.UtcNow
                };

                // Save the new payment method to database
                var savedMethod = await _orderRepository.CreatePaymentMethodAsync(newPaymentMethod);
                return savedMethod;
            }
            catch (Exception ex)
            {
                // If payment method creation fails, return a default one
                // This is a fallback to prevent order creation from failing
                return new PaymentMethods
                {
                    Id = 1, // Use a default ID
                    MethodName = "Cash on Delivery",
                    MethodType = "Standard",
                    IsActive = true,
                    DisplayName = "Cash on Delivery",
                    CreatedAt = DateTime.UtcNow
                };
            }
        }
    }
}
