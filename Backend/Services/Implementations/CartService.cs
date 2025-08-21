using AutoMapper;
using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Cart;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
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
                cartItem.LineTotal = cartItem.Quantity * cartItem.UnitPrice;
                await _cartRepository.UpdateCartItemAsync(cartItem);
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = addToCartDto.ProductId,
                    Quantity = addToCartDto.Quantity,
                    UnitPrice = product.Price ?? 0,
                    LineTotal = addToCartDto.Quantity * (product.Price ?? 0)
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
            cartItem.LineTotal = cartItem.Quantity * cartItem.UnitPrice;
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
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
            if (cart == null)
            {
                return new PagedCartResultDTO
                {
                    Page = page,
                    PageSize = pageSize,
                };
            }

            var pagedResult = await _cartRepository.GetPagedCartItemsAsync(cart.Id, page, pageSize);
            var cartSummary = await GetCartSummaryAsync(userId);

            return new PagedCartResultDTO
            {
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                TotalItems = pagedResult.TotalItems,
                TotalPages = pagedResult.TotalPages,
                CartItems = pagedResult.Items.Select(MapCartItemToDTO).ToList(),
                CartSummary = cartSummary
            };
        }

        public async Task<CartSummaryDTO> GetCartSummaryAsync(int userId)
        {
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
            if (cart == null)
            {
                return new CartSummaryDTO();
            }

            return new CartSummaryDTO
            {
                CartId = cart.Id,
                TotalItems = await _cartRepository.GetCartItemsCountAsync(cart.Id),
                TotalAmount = await _cartRepository.GetCartTotalAsync(cart.Id)
            };
        }

        public async Task<int> GetCartItemsCountAsync(int userId)
        {
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
            return cart == null ? 0 : await _cartRepository.GetCartItemsCountAsync(cart.Id);
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
