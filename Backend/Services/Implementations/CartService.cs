using AutoMapper;
using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Cart;
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
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IMapper mapper,
            ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
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
    }
}
