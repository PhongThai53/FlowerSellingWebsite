using AutoMapper;
using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Order;
using FlowerSellingWebsite.Models.DTOs.Cart;
using FlowerSellingWebsite.Models.DTOs.Order;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProductFlowersRepository _productFlowersRepository;
        private readonly ISupplierListingsRepository _supplierListingsRepository;
        private readonly IPurchaseOrdersRepository _purchaseOrdersRepository;
        private readonly IPurchaseOrderDetailsRepository _purchaseOrderDetailsRepository;
        private readonly IVNPayService _vnPayService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IProductFlowersRepository productFlowersRepository,
            ISupplierListingsRepository supplierListingsRepository,
            IPurchaseOrdersRepository purchaseOrdersRepository,
            IPurchaseOrderDetailsRepository purchaseOrderDetailsRepository,
            IVNPayService vnPayService,
            IConfiguration configuration,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _productFlowersRepository = productFlowersRepository;
            _supplierListingsRepository = supplierListingsRepository;
            _purchaseOrdersRepository = purchaseOrdersRepository;
            _purchaseOrderDetailsRepository = purchaseOrderDetailsRepository;
            _vnPayService = vnPayService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PagedResult<OrderDTO>> GetOrderHistoryAsync(UrlQueryParams urlQueryParams, int? customerId = null)
        {
            // Implementation for getting order history
            throw new NotImplementedException("GetOrderHistoryAsync not implemented yet");
        }

        public async Task<OrderDTO?> GetOrderByIdAsync(int orderId)
        {
            // Implementation for getting order by ID
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

        public async Task<CheckoutResponseDTO> ProcessCheckoutAsync(CheckoutRequestDTO checkoutRequest, int customerId, string clientIpAddress = null)
        {
            try
            {
                _logger.LogInformation("Starting checkout process for customer {CustomerId}", customerId);

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

                // Validate cart items and flower availability
                var validationResult = await ValidateCartItemsFlowerAvailabilityAsync(checkoutRequest.CartItems);
                if (!validationResult.IsValid)
                {
                    return new CheckoutResponseDTO
                    {
                        Succeeded = false,
                        Message = validationResult.ErrorMessage
                    };
                }

                // Calculate total costs based on flower availability
                var costCalculation = await CalculateProductCostsAsync(checkoutRequest.CartItems);
                if (!costCalculation.IsValid)
                {
                    return new CheckoutResponseDTO
                    {
                        Succeeded = false,
                        Message = costCalculation.ErrorMessage
                    };
                }

                // Generate order number
                var orderNumber = GenerateOrderNumber();

                // Create order
                var order = new Orders
                {
                    OrderNumber = orderNumber,
                    CustomerId = customerId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Created",
                    Subtotal = costCalculation.Subtotal,
                    DiscountAmount = checkoutRequest.DiscountAmount ?? 0,
                    TaxAmount = costCalculation.TaxAmount,
                    ShippingFee = checkoutRequest.ShippingFee ?? 0,
                    TotalAmount = costCalculation.TotalAmount,
                    PaymentStatus = "Pending",
                    ShippingAddress = checkoutRequest.ShippingAddress,
                    BillingAddress = checkoutRequest.BillingAddress,
                    Notes = checkoutRequest.Notes,
                    CustomerFirstName = customer.FullName,
                    CustomerLastName = string.Empty,
                    CustomerEmail = customer.Email,
                    CustomerPhone = customer.Phone,
                    CompanyName = null,
                    Country = null,
                    City = checkoutRequest.City,
                    State = checkoutRequest.State,
                    Postcode = null,
                    StreetAddress = checkoutRequest.StreetAddress,
                    StreetAddress2 = checkoutRequest.StreetAddress2
                };

                var createdOrder = await _orderRepository.CreateOrderAsync(order);

                // Create order details
                foreach (var item in costCalculation.ProductCosts)
                {
                    var orderDetail = new OrderDetails
                    {
                        OrderId = createdOrder.Id,
                        ProductId = item.ProductId,
                        ItemName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.LineTotal
                    };
                    await _orderRepository.CreateOrderDetailAsync(orderDetail);
                }

                // Allocate immediately for COD, defer for VNPay until payment success
                if (!string.Equals(checkoutRequest.PaymentMethod, "vnpay", StringComparison.OrdinalIgnoreCase))
                {
                    await CreatePurchaseOrdersAsync(costCalculation.FlowerRequirements, createdOrder.Id);
                }
                // For VNPay orders, keep status as "Created" until payment is confirmed

                // Clear cart
                await _cartRepository.ClearCartByUserIdAsync(customerId);

                _logger.LogInformation("Checkout completed successfully for order {OrderId}", createdOrder.Id);

                var response = new CheckoutResponseDTO
                {
                    Succeeded = true,
                    Message = "Order created successfully",
                    Data = new CheckoutResponseDataDTO
                    {
                        OrderId = createdOrder.Id,
                        OrderNumber = orderNumber,
                        TotalAmount = costCalculation.TotalAmount,
                        OrderStatus = "Created",
                        PaymentStatus = "Pending"
                    }
                };

                // If VNPay selected, generate payment URL
                if (string.Equals(checkoutRequest.PaymentMethod, "vnpay", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var returnUrl = _configuration["VNPay:ReturnUrl"] ?? string.Empty;
                        var cancelUrl = _configuration["VNPay:CancelUrl"] ?? string.Empty;
                        var paymentUrl = await _vnPayService.CreatePaymentUrlAsync(
                            createdOrder.Id,
                            orderNumber,
                            costCalculation.TotalAmount,
                            returnUrl,
                            cancelUrl,
                            clientIpAddress
                        );
                        response.Data.PaymentUrl = paymentUrl;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create VNPay URL for order {OrderId}", createdOrder.Id);
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout for customer {CustomerId}", customerId);
                return new CheckoutResponseDTO
                {
                    Succeeded = false,
                    Message = "An error occurred while processing checkout"
                };
            }
        }

        public async Task<CheckoutResponseDTO> ConfirmCODOrderAsync(int orderId, int customerId)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return new CheckoutResponseDTO
                    {
                        Succeeded = false,
                        Message = "Order not found"
                    };
                }

                if (order.CustomerId != customerId)
                {
                    return new CheckoutResponseDTO
                    {
                        Succeeded = false,
                        Message = "Unauthorized access to order"
                    };
                }

                // Update order status
                await _orderRepository.UpdateOrderStatusAsync(orderId, "Confirmed");
                await _orderRepository.UpdatePaymentStatusAsync(orderId, "Pending");

                return new CheckoutResponseDTO
                {
                    Succeeded = true,
                    Message = "COD order confirmed successfully",
                    Data = new CheckoutResponseDataDTO
                    {
                        OrderId = orderId,
                        OrderNumber = order.OrderNumber,
                        TotalAmount = order.TotalAmount,
                        OrderStatus = "Confirmed",
                        PaymentStatus = "Pending"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming COD order {OrderId}", orderId);
                return new CheckoutResponseDTO
                {
                    Succeeded = false,
                    Message = "An error occurred while confirming order"
                };
            }
        }

        public async Task<bool> ConfirmPaidOrderAsync(int orderId)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(orderId);
                if (order == null) return false;

                // Only allocate if not already allocated
                if (order.Status == "Created")
                {
                    // For VNPay orders, we need to allocate stock after payment confirmation
                    // This method is called after successful payment
                }

                if (order.OrderDetails == null || !order.OrderDetails.Any())
                {
                    // Load order with details if repository method returns without
                    order = await _orderRepository.GetOrderWithDetailsAsync(orderId) ?? order;
                }

                var cartItems = new List<CheckoutCartItemDTO>();
                if (order.OrderDetails != null)
                {
                    foreach (var d in order.OrderDetails)
                    {
                        cartItems.Add(new CheckoutCartItemDTO
                        {
                            ProductId = d.ProductId,
                            Quantity = d.Quantity,
                            UnitPrice = d.UnitPrice,
                            LineTotal = d.LineTotal,
                            ProductName = d.ItemName
                        });
                    }
                }

                if (!cartItems.Any()) return false;

                // Recalculate allocations based on current supplier listings
                var calc = await CalculateProductCostsAsync(cartItems);
                if (!calc.IsValid) return false;

                await CreatePurchaseOrdersAsync(calc.FlowerRequirements, order.Id);
                // Keep order status as "Created" - no need to change status
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming paid order {OrderId}", orderId);
                return false;
            }
        }

        private async Task<FlowerAvailabilityValidationResult> ValidateCartItemsFlowerAvailabilityAsync(List<CheckoutCartItemDTO> cartItems)
        {
            var result = new FlowerAvailabilityValidationResult { IsValid = true };

            foreach (var cartItem in cartItems)
            {
                var product = await _productRepository.GetProductByIdAsync(cartItem.ProductId);
                if (product == null)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Product {cartItem.ProductId} not found";
                    return result;
                }

                // Get flower requirements for this product
                var flowerRequirements = await _productFlowersRepository.GetFlowerRequirementsForProductAsync(cartItem.ProductId);
                
                foreach (var flowerReq in flowerRequirements)
                {
                    var totalFlowersNeeded = flowerReq.QuantityNeeded * cartItem.Quantity;
                    var availableFlowers = await _supplierListingsRepository.GetAvailableFlowersByFlowerIdAsync(flowerReq.FlowerId);
                    
                    var totalAvailable = availableFlowers.Sum(sl => sl.AvailableQuantity);
                    
                    if (totalAvailable < totalFlowersNeeded)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Insufficient flowers for product {product.Name}. Need {totalFlowersNeeded} {flowerReq.Flower.Name}, but only {totalAvailable} available.";
                        return result;
                    }
                }
            }

            return result;
        }

        private async Task<ProductCostCalculationResult> CalculateProductCostsAsync(List<CheckoutCartItemDTO> cartItems)
        {
            var result = new ProductCostCalculationResult 
            { 
                IsValid = true,
                ProductCosts = new List<ProductCostInfo>(),
                FlowerRequirements = new List<FlowerRequirementInfo>()
            };

            // Step 1: Calculate total flower requirements for entire cart
            var totalFlowerRequirements = new Dictionary<int, int>(); // FlowerId -> Total Quantity Needed
            var productFlowerMappings = new Dictionary<int, List<ProductFlowers>>(); // ProductId -> Flower Requirements

            foreach (var cartItem in cartItems)
            {
                var flowerRequirements = await _productFlowersRepository.GetFlowerRequirementsForProductAsync(cartItem.ProductId);
                productFlowerMappings[cartItem.ProductId] = flowerRequirements.ToList();

                foreach (var flowerReq in flowerRequirements)
                {
                    var totalNeeded = flowerReq.QuantityNeeded * cartItem.Quantity;
                    if (totalFlowerRequirements.ContainsKey(flowerReq.FlowerId))
                    {
                        totalFlowerRequirements[flowerReq.FlowerId] += totalNeeded;
                    }
                    else
                    {
                        totalFlowerRequirements[flowerReq.FlowerId] = totalNeeded;
                    }
                }
            }

            // Step 2: Allocate flowers sequentially from cheapest suppliers for entire cart
            var flowerAllocations = new Dictionary<int, List<SequentialAllocation>>(); // FlowerId -> Sequential Allocations

            foreach (var flowerReq in totalFlowerRequirements)
            {
                var flowerId = flowerReq.Key;
                var totalQuantityNeeded = flowerReq.Value;

                var suppliers = await _supplierListingsRepository.GetAvailableFlowersByFlowerIdAsync(flowerId);
                var availableSuppliers = suppliers
                    .Where(sl => sl.AvailableQuantity > 0)
                    .OrderBy(sl => sl.UnitPrice)
                    .ToList();

                if (!availableSuppliers.Any())
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"No available suppliers for flower ID {flowerId}";
                    return result;
                }

                var allocations = new List<SequentialAllocation>();
                int remainingNeeded = totalQuantityNeeded;
                int allocationIndex = 0;

                foreach (var supplier in availableSuppliers)
                {
                    if (remainingNeeded <= 0) break;

                    int quantityToTake = Math.Min(remainingNeeded, supplier.AvailableQuantity);
                    var allocation = new SequentialAllocation
                    {
                        Index = allocationIndex,
                        SupplierId = supplier.SupplierId,
                        SupplierListingId = supplier.Id, // Add this line to track the actual supplier listing
                        SupplierName = supplier.Supplier?.SupplierName ?? "Unknown",
                        FlowerId = flowerId,
                        Quantity = quantityToTake,
                        UnitPrice = supplier.UnitPrice,
                        LineTotal = quantityToTake * supplier.UnitPrice
                    };

                    allocations.Add(allocation);
                    remainingNeeded -= quantityToTake;
                    allocationIndex++;
                }

                if (remainingNeeded > 0)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Insufficient flowers for flower ID {flowerId}. Need: {totalQuantityNeeded}, Available: {totalQuantityNeeded - remainingNeeded}";
                    return result;
                }

                flowerAllocations[flowerId] = allocations;
            }

            // Step 3: Calculate price for each cart item based on sequential allocation
            decimal subtotal = 0;
            const decimal TAX_RATE = 0.5m; // 50% service fee

            foreach (var cartItem in cartItems)
            {
                var product = await _productRepository.GetProductByIdAsync(cartItem.ProductId);
                var flowerRequirements = productFlowerMappings[cartItem.ProductId];
                
                decimal productTotalCost = 0;
                var flowerReqs = new List<FlowerRequirementInfo>();

                foreach (var flowerReq in flowerRequirements)
                {
                    var totalFlowersNeeded = flowerReq.QuantityNeeded * cartItem.Quantity;
                    var allocations = flowerAllocations[flowerReq.FlowerId];

                    // Calculate cost for this product's flower requirement
                    decimal flowerCost = 0;
                    int flowersAllocated = 0;
                    var flowerReqInfo = new FlowerRequirementInfo
                    {
                        FlowerId = flowerReq.FlowerId,
                        FlowerName = flowerReq.Flower.Name,
                        TotalQuantityNeeded = totalFlowersNeeded,
                        SupplierAllocations = new List<SupplierAllocationInfo>()
                    };

                    foreach (var allocation in allocations)
                    {
                        if (flowersAllocated >= totalFlowersNeeded) break;

                        int quantityToTake = Math.Min(totalFlowersNeeded - flowersAllocated, allocation.Quantity);
                        decimal supplierCost = quantityToTake * allocation.UnitPrice;
                        
                        flowerCost += supplierCost;
                        flowersAllocated += quantityToTake;

                        flowerReqInfo.SupplierAllocations.Add(new SupplierAllocationInfo
                        {
                            SupplierListingId = allocation.SupplierListingId, // Use the actual supplier listing ID
                            SupplierId = allocation.SupplierId,
                            FlowerId = flowerReq.FlowerId, // Add the flower ID
                            SupplierName = allocation.SupplierName,
                            Quantity = quantityToTake,
                            UnitPrice = allocation.UnitPrice,
                            LineTotal = supplierCost
                        });
                    }
                    
                    productTotalCost += flowerCost;
                    flowerReqs.Add(flowerReqInfo);
                }

                var productCostInfo = new ProductCostInfo
                {
                    ProductId = cartItem.ProductId,
                    ProductName = product.Name,
                    Quantity = cartItem.Quantity,
                    UnitPrice = productTotalCost / cartItem.Quantity,
                    LineTotal = productTotalCost
                };

                result.ProductCosts.Add(productCostInfo);
                result.FlowerRequirements.AddRange(flowerReqs);
                subtotal += productTotalCost;
            }

            var taxAmount = subtotal * TAX_RATE;
            result.Subtotal = subtotal;
            result.TaxAmount = taxAmount;
            result.TotalAmount = subtotal + taxAmount;

            return result;
        }

        private async Task CreatePurchaseOrdersAsync(List<FlowerRequirementInfo> flowerRequirements, int orderId)
        {
            // Consolidate flower requirements by supplier listing to avoid duplicate deductions
            var consolidatedAllocations = new Dictionary<int, ConsolidatedAllocation>();
            
            foreach (var flowerReq in flowerRequirements)
            {
                foreach (var allocation in flowerReq.SupplierAllocations)
                {
                    var key = allocation.SupplierListingId;
                    if (!consolidatedAllocations.ContainsKey(key))
                    {
                        consolidatedAllocations[key] = new ConsolidatedAllocation
                        {
                            SupplierListingId = allocation.SupplierListingId,
                            SupplierId = allocation.SupplierId,
                            FlowerId = allocation.FlowerId,
                            TotalQuantity = 0,
                            UnitPrice = allocation.UnitPrice,
                            TotalAmount = 0
                        };
                    }
                    
                    consolidatedAllocations[key].TotalQuantity += allocation.Quantity;
                    consolidatedAllocations[key].TotalAmount += allocation.LineTotal;
                }
            }

            // Group by supplier
            var supplierGroups = consolidatedAllocations.Values
                .GroupBy(x => x.SupplierId)
                .ToList();

            foreach (var supplierGroup in supplierGroups)
            {
                var supplierId = supplierGroup.Key;
                var totalAmount = supplierGroup.Sum(x => x.TotalAmount);

                var purchaseOrder = new PurchaseOrders
                {
                    PurchaseOrderNumber = GeneratePurchaseOrderNumber(),
                    SupplierId = supplierId,
                    CreatedDate = DateTime.UtcNow,
                    Status = "pending",
                    TotalAmount = totalAmount,
                    Notes = $"Generated from order {orderId}"
                };

                var createdPO = await _purchaseOrdersRepository.createAsync(purchaseOrder);

                // Create purchase order details
                foreach (var item in supplierGroup)
                {
                    var poDetail = new PurchaseOrderDetails
                    {
                        PurchaseOrderId = createdPO.Id,
                        FlowerId = item.FlowerId,
                        Quantity = item.TotalQuantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.TotalAmount
                    };

                    await _purchaseOrderDetailsRepository.createAsync(poDetail);

                    // Deduct from supplier listing stock (now consolidated, so only deduct once per listing)
                    if (item.SupplierListingId > 0)
                    {
                        await _supplierListingsRepository.DeductAvailableQuantityAsync(
                            item.SupplierListingId,
                            item.TotalQuantity
                        );
                    }
                }
            }
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private string GeneratePurchaseOrderNumber()
        {
            return $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }

    // Helper classes for the new flow
    public class FlowerAvailabilityValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class ProductCostCalculationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<ProductCostInfo> ProductCosts { get; set; } = new List<ProductCostInfo>();
        public List<FlowerRequirementInfo> FlowerRequirements { get; set; } = new List<FlowerRequirementInfo>();
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ProductCostInfo
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class FlowerRequirementInfo
    {
        public int FlowerId { get; set; }
        public string FlowerName { get; set; } = string.Empty;
        public int TotalQuantityNeeded { get; set; }
        public List<SupplierAllocationInfo> SupplierAllocations { get; set; } = new List<SupplierAllocationInfo>();
    }

    public class SupplierAllocationInfo
    {
        public int SupplierListingId { get; set; }
        public int SupplierId { get; set; }
        public int FlowerId { get; set; } // Add FlowerId to track which flower this allocation is for
        public string SupplierName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    // Helper class for sequential allocation (same as in CartService)
    public class SequentialAllocation
    {
        public int Index { get; set; }
        public int SupplierId { get; set; }
        public int SupplierListingId { get; set; } // Add this property to track the actual supplier listing ID
        public string SupplierName { get; set; } = string.Empty;
        public int FlowerId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    // Helper class for consolidated allocations
    public class ConsolidatedAllocation
    {
        public int SupplierListingId { get; set; }
        public int SupplierId { get; set; }
        public int FlowerId { get; set; }
        public int TotalQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
