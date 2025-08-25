using AutoMapper;
using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Cart;
using FlowerSellingWebsite.Models.DTOs.Product;
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
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductFlowersRepository _productFlowersRepository;
        private readonly ISupplierListingsRepository _supplierListingsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IProductFlowersRepository productFlowersRepository,
            ISupplierListingsRepository supplierListingsRepository,
            IMapper mapper,
            ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _productFlowersRepository = productFlowersRepository;
            _supplierListingsRepository = supplierListingsRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CartDTO> GetCartByUserIdAsync(int userId)
        {
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);
            if (cart == null)
            {
                // Create a new cart if one doesn't exist
                var newCart = new Cart { UserId = userId };
                await _cartRepository.createAsync(newCart);
                cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);
            }
            return MapCartToDTO(cart!);
        }
        
        public async Task<CartDTO> AddToCartAsync(int userId, AddToCartDTO addToCartDto)
        {
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId) ?? new Cart { UserId = userId };
            if (cart.Id == 0)
            {
                await _cartRepository.createAsync(cart);
            }

            var product = await _productRepository.GetProductByIdAsync(addToCartDto.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            var cartItem = await _cartRepository.GetCartItemAsync(cart.Id, addToCartDto.ProductId);
            if (cartItem != null)
            {
                cartItem.Quantity += addToCartDto.Quantity;
                // LineTotal is computed by the database, don't set it manually
                await _cartRepository.UpdateCartItemAsync(cartItem);
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = addToCartDto.ProductId,
                    Quantity = addToCartDto.Quantity,
                    UnitPrice = product.Price ?? 0
                    // LineTotal is computed by the database automatically
                };
                await _cartRepository.AddCartItemAsync(cartItem);
            }
            
            var updatedCart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);
            return MapCartToDTO(updatedCart!);
        }

        public async Task<CartItemDTO> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemDTO updateDto)
        {
            // Load cart WITH items to ensure CartItems collection is populated
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);
            if (cart == null)
            {
                throw new UnauthorizedAccessException("Cart not found for the user.");
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                throw new KeyNotFoundException("Cart item not found.");
            }

            cartItem.Quantity = updateDto.Quantity;
            // LineTotal is computed by the database, don't set it manually
            await _cartRepository.UpdateCartItemAsync(cartItem);
            
            return MapCartItemToDTO(cartItem);
        }

        public async Task<bool> RemoveCartItemAsync(int userId, int cartItemId)
        {
            // Load cart WITH items to ensure CartItems collection is populated
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);
            if (cart == null)
            {
                throw new UnauthorizedAccessException("Cart not found for the user.");
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                throw new KeyNotFoundException("Cart item not found.");
            }

            await _cartRepository.DeleteCartItemAsync(cartItem);
            return true;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
            if (cart == null)
            {
                return true; // Cart is already empty/non-existent
            }
            await _cartRepository.ClearCartAsync(cart.Id);
            return true;
        }

        public async Task<PagedCartResultDTO> GetPagedCartItemsAsync(int userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
                if (cart == null)
                {
                    // Return empty result when no active cart exists
                    return new PagedCartResultDTO
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalItems = 0,
                        TotalPages = 0,
                        CartItems = new List<CartItemDTO>(),
                        CartSummary = new CartSummaryDTO
                        {
                            CartId = 0,
                            TotalItems = 0,
                            TotalAmount = 0
                        }
                    };
                }

                var pagedResult = await _cartRepository.GetPagedCartItemsAsync(cart.Id, page, pageSize);
                var cartSummary = await GetCartSummaryAsync(userId);

                // Map cart items with proper product information
                var cartItemDTOs = pagedResult.Items.Select(item => new CartItemDTO
                {
                    Id = item.Id,
                    CartId = item.CartId,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? "Unknown Product",
                    ProductUrl = item.Product?.Url ?? "",
                    ProductImage = item.Product?.ProductPhotos?.FirstOrDefault(pp => !pp.IsDeleted)?.Url,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineTotal,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                }).ToList();

                return new PagedCartResultDTO
                {
                    Page = pagedResult.Page,
                    PageSize = pagedResult.PageSize,
                    TotalItems = pagedResult.TotalItems,
                    TotalPages = pagedResult.TotalPages,
                    CartItems = cartItemDTOs,
                    CartSummary = cartSummary
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting paged cart items for user {userId}: {ex.Message}");
                // Return empty result on error
                return new PagedCartResultDTO
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = 0,
                    TotalPages = 0,
                    CartItems = new List<CartItemDTO>(),
                    CartSummary = new CartSummaryDTO
                    {
                        CartId = 0,
                        TotalItems = 0,
                        TotalAmount = 0
                    }
                };
            }
        }

        public async Task<CartSummaryDTO> GetCartSummaryAsync(int userId)
        {
            try
            {
                var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
                if (cart == null)
                {
                    // Return empty cart summary when no active cart exists
                    return new CartSummaryDTO
                    {
                        CartId = 0,
                        TotalItems = 0,
                        TotalAmount = 0
                    };
                }

                return new CartSummaryDTO
                {
                    CartId = cart.Id,
                    TotalItems = await _cartRepository.GetCartItemsCountAsync(cart.Id),
                    TotalAmount = await _cartRepository.GetCartTotalAsync(cart.Id)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cart summary for user {userId}: {ex.Message}");
                // Return empty cart summary on error
                return new CartSummaryDTO
                {
                    CartId = 0,
                    TotalItems = 0,
                    TotalAmount = 0
                };
            }
        }

        public async Task<int> GetCartItemsCountAsync(int userId)
        {
            try
            {
                var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
                if (cart == null)
                {
                    return 0;
                }
                return await _cartRepository.GetCartItemsCountAsync(cart.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cart items count for user {userId}: {ex.Message}");
                return 0;
            }
        }

        public async Task<CartDTO> EnsureActiveCartAsync(int userId)
        {
            try
            {
                var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
                if (cart == null)
                {
                    // Create a new cart for the user
                    cart = await _cartRepository.CreateNewCartForUserAsync(userId);
                    Console.WriteLine($"Created new cart {cart.Id} for user {userId}");
                }

                return MapCartToDTO(cart);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring active cart for user {userId}: {ex.Message}");
                throw;
            }
        }

        private CartDTO MapCartToDTO(Cart cart)
        {
            var cartDto = _mapper.Map<CartDTO>(cart);
            cartDto.CartItems = cart.CartItems.Select(MapCartItemToDTO).ToList();
            return cartDto;
        }

        private CartItemDTO MapCartItemToDTO(CartItem cartItem)
        {
            return _mapper.Map<CartItemDTO>(cartItem);
        }

        public async Task<CartPriceCalculationDTO> CalculateCartPriceAsync(int userId)
        {
            try
            {
                var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);
                if (cart == null || !cart.CartItems.Any())
                {
                    return new CartPriceCalculationDTO
                    {
                        IsValid = false,
                        Message = "Cart is empty"
                    };
                }

                var result = new CartPriceCalculationDTO
                {
                    IsValid = true,
                    CartItems = new List<CartItemPriceInfo>()
                };

                decimal subtotal = 0;

                foreach (var cartItem in cart.CartItems)
                {
                    var product = await _productRepository.GetProductByIdAsync(cartItem.ProductId);
                    if (product == null) continue;

                    // Kiểm tra availability và tính giá từ supplier
                    var availability = await CheckProductAvailabilityWithPricingAsync(cartItem.ProductId, cartItem.Quantity);
                    
                    if (!availability.IsAvailable)
                    {
                        result.IsValid = false;
                        result.Message = $"Product {product.Name} is not available in requested quantity";
                        break;
                    }

                    var cartItemInfo = new CartItemPriceInfo
                    {
                        CartItemId = cartItem.Id,
                        ProductId = cartItem.ProductId,
                        ProductName = product.Name,
                        Quantity = cartItem.Quantity,
                        OriginalUnitPrice = product.Price ?? 0,
                        CalculatedUnitPrice = availability.CalculatedUnitPrice,
                        LineTotal = availability.CalculatedUnitPrice * cartItem.Quantity,
                        PriceDifference = (availability.CalculatedUnitPrice - (product.Price ?? 0)) * cartItem.Quantity,
                        SupplierBreakdown = availability.SupplierBreakdown
                    };

                    result.CartItems.Add(cartItemInfo);
                    subtotal += cartItemInfo.LineTotal;
                }

                if (result.IsValid)
                {
                    result.Subtotal = subtotal;
                    result.ServiceFee = subtotal * 0.5m; // 50% service fee
                    result.TotalAmount = result.Subtotal + result.ServiceFee;
                    result.Message = "Cart prices calculated successfully";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating cart price for user {UserId}", userId);
                return new CartPriceCalculationDTO
                {
                    IsValid = false,
                    Message = $"Error calculating prices: {ex.Message}"
                };
            }
        }

        // Thêm validation methods mới
        public async Task<CartValidationResultDTO> ValidateCartItemQuantityAsync(int userId, int productId, int quantity)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return new CartValidationResultDTO
                    {
                        IsValid = false,
                        Message = "Product not found"
                    };
                }

                // Kiểm tra availability
                var availability = await CheckProductAvailabilityWithPricingAsync(productId, quantity);
                
                var validationInfo = new CartItemValidationInfo
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    RequestedQuantity = quantity,
                    MaxAvailableQuantity = availability.MaxAvailableQuantity,
                    IsAvailable = availability.IsAvailable,
                    Message = availability.IsAvailable ? "Available" : availability.Message,
                    UnitPrice = availability.CalculatedUnitPrice,
                    TotalPrice = availability.CalculatedUnitPrice * quantity
                };

                return new CartValidationResultDTO
                {
                    IsValid = availability.IsAvailable,
                    Message = availability.IsAvailable ? "Quantity is valid" : availability.Message,
                    CartItems = new List<CartItemValidationInfo> { validationInfo },
                    MaxQuantityAllowed = availability.MaxAvailableQuantity,
                    TotalPrice = validationInfo.TotalPrice
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cart item quantity for product {ProductId}", productId);
                return new CartValidationResultDTO
                {
                    IsValid = false,
                    Message = $"Error during validation: {ex.Message}"
                };
            }
        }

        public async Task<CartValidationResultDTO> ValidateEntireCartAsync(int userId)
        {
            try
            {
                var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);
                if (cart == null || !cart.CartItems.Any())
                {
                    return new CartValidationResultDTO
                    {
                        IsValid = false,
                        Message = "Cart is empty"
                    };
                }

                var result = new CartValidationResultDTO
                {
                    IsValid = true,
                    CartItems = new List<CartItemValidationInfo>()
                };

                decimal totalPrice = 0;

                foreach (var cartItem in cart.CartItems)
                {
                    var validation = await ValidateCartItemQuantityAsync(userId, cartItem.ProductId, cartItem.Quantity);
                    if (!validation.IsValid)
                    {
                        result.IsValid = false;
                    }
                    
                    result.CartItems.AddRange(validation.CartItems);
                    totalPrice += validation.CartItems.Sum(ci => ci.TotalPrice);
                }

                result.TotalPrice = totalPrice;
                result.Message = result.IsValid ? "All cart items are valid" : "Some cart items are invalid";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating entire cart for user {UserId}", userId);
                return new CartValidationResultDTO
                {
                    IsValid = false,
                    Message = $"Error during validation: {ex.Message}"
                };
            }
        }

        // Helper method để kiểm tra availability và tính giá
        private async Task<ProductAvailabilityWithPricingDTO> CheckProductAvailabilityWithPricingAsync(int productId, int quantity)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return new ProductAvailabilityWithPricingDTO
                    {
                        IsAvailable = false,
                        ProductId = productId,
                        Message = "Product not found"
                    };
                }

                var flowerRequirements = await _productFlowersRepository.GetFlowerRequirementsForProductAsync(productId);
                if (flowerRequirements == null || !flowerRequirements.Any())
                {
                    return new ProductAvailabilityWithPricingDTO
                    {
                        IsAvailable = false,
                        ProductId = productId,
                        ProductName = product.Name,
                        RequestedQuantity = quantity,
                        Message = "Product has no flower requirements"
                    };
                }

                var result = new ProductAvailabilityWithPricingDTO
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    RequestedQuantity = quantity,
                    SupplierBreakdown = new List<SupplierPriceBreakdown>()
                };

                int maxAvailableQuantity = int.MaxValue;
                bool isAvailable = true;
                decimal totalCost = 0;

                foreach (var flowerReq in flowerRequirements)
                {
                    var totalFlowersNeeded = flowerReq.QuantityNeeded * quantity;
                    
                    // Lấy tất cả suppliers cho flower này (không filter AvailableQuantity)
                    var allSuppliers = await _supplierListingsRepository.GetByFlowerIdAsync(flowerReq.FlowerId);
                    
                    if (allSuppliers == null || !allSuppliers.Any())
                    {
                        isAvailable = false;
                        result.Message = $"No suppliers found for flower {flowerReq.Flower?.Name}";
                        break;
                    }

                    // Lọc suppliers có AvailableQuantity > 0 và Status = "available"
                    var availableSuppliers = allSuppliers
                        .Where(sl => sl.AvailableQuantity > 0 && sl.Status == "available")
                        .OrderBy(sl => sl.UnitPrice) // Sắp xếp theo giá từ thấp đến cao
                        .ToList();

                    if (!availableSuppliers.Any())
                    {
                        isAvailable = false;
                        result.Message = $"No available suppliers for flower {flowerReq.Flower?.Name}";
                        break;
                    }

                    // Tính toán số lượng có thể lấy từ từng supplier
                    int remainingNeeded = totalFlowersNeeded;
                    decimal flowerCost = 0;
                    var flowerSupplierBreakdown = new List<SupplierPriceBreakdown>();

                    foreach (var supplier in availableSuppliers)
                    {
                        if (remainingNeeded <= 0) break;

                        int quantityToTake = Math.Min(remainingNeeded, supplier.AvailableQuantity);
                        decimal supplierCost = quantityToTake * supplier.UnitPrice;
                        
                        flowerCost += supplierCost;
                        remainingNeeded -= quantityToTake;

                        flowerSupplierBreakdown.Add(new SupplierPriceBreakdown
                        {
                            SupplierId = supplier.SupplierId,
                            SupplierName = supplier.Supplier?.SupplierName ?? "Unknown",
                            FlowerId = flowerReq.FlowerId,
                            FlowerName = flowerReq.Flower?.Name ?? "Unknown",
                            QuantityNeeded = quantityToTake,
                            UnitPrice = supplier.UnitPrice,
                            LineTotal = supplierCost
                        });

                        if (remainingNeeded <= 0) break;
                    }

                    // Nếu không đủ hoa từ tất cả suppliers
                    if (remainingNeeded > 0)
                    {
                        isAvailable = false;
                        result.Message = $"Insufficient flowers for {flowerReq.Flower?.Name}. Need: {totalFlowersNeeded}, Available: {totalFlowersNeeded - remainingNeeded}";
                        break;
                    }

                    // Tính số lượng tối đa có thể làm được cho flower này
                    int maxQuantityForThisFlower = availableSuppliers.Sum(sl => sl.AvailableQuantity) / flowerReq.QuantityNeeded;
                    maxAvailableQuantity = Math.Min(maxAvailableQuantity, maxQuantityForThisFlower);

                    totalCost += flowerCost;
                    result.SupplierBreakdown.AddRange(flowerSupplierBreakdown);
                }

                result.IsAvailable = isAvailable;
                result.MaxAvailableQuantity = maxAvailableQuantity;
                result.TotalCost = totalCost;
                result.CalculatedUnitPrice = isAvailable ? totalCost / quantity : 0;

                if (isAvailable)
                {
                    result.Message = $"Available for {quantity} products. Max available: {maxAvailableQuantity}";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product availability with pricing for product {ProductId}", productId);
                return new ProductAvailabilityWithPricingDTO
                {
                    IsAvailable = false,
                    ProductId = productId,
                    Message = $"Error during availability check: {ex.Message}"
                };
            }
        }

        // Helper methods for flower requirements and supplier listings
        private async Task<IEnumerable<ProductFlowers>> GetFlowerRequirementsForProductAsync(int productId)
        {
            return await _productFlowersRepository.GetProductFlowers(productId);
        }

        private async Task<IEnumerable<SupplierListings>> GetAvailableFlowersByFlowerIdAsync(int flowerId)
        {
            return await _supplierListingsRepository.GetByFlowerIdAsync(flowerId);
        }
    }
}
