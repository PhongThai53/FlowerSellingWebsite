using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Cart;
using FlowerSellingWebsite.Services.Interfaces;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowerSellingWebsite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, IUserRepository userRepository, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var cart = await _cartService.GetCartByUserIdAsync(userId);
                return Ok(ApiResponse<CartDTO>.Ok(cart, "Cart retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart");
                return StatusCode(500, ApiResponse<CartDTO>.Fail("An error occurred while retrieving the cart"));
            }
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetCartItems(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var result = await _cartService.GetPagedCartItemsAsync(userId, page, pageSize);
                return Ok(ApiResponse<PagedCartResultDTO>.Ok(result, "Cart items retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart items");
                return StatusCode(500, ApiResponse<PagedCartResultDTO>.Fail("An error occurred while retrieving cart items"));
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetCartSummary()
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var summary = await _cartService.GetCartSummaryAsync(userId);
                return Ok(ApiResponse<CartSummaryDTO>.Ok(summary, "Cart summary retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart summary");
                return StatusCode(500, ApiResponse<CartSummaryDTO>.Fail("An error occurred while retrieving cart summary"));
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCartItemsCount()
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var count = await _cartService.GetCartItemsCountAsync(userId);
                return Ok(ApiResponse<int>.Ok(count, "Cart items count retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart items count");
                return StatusCode(500, ApiResponse<int>.Fail("An error occurred while retrieving cart items count"));
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDTO addToCartDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<CartDTO>.Fail("Invalid request data"));
                }

                var userId = await GetCurrentUserIdAsync();
                var cart = await _cartService.AddToCartAsync(userId, addToCartDto);
                return Ok(ApiResponse<CartDTO>.Ok(cart, "Item added to cart successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<CartDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return StatusCode(500, ApiResponse<CartDTO>.Fail("An error occurred while adding item to cart"));
            }
        }

        [HttpPut("items/{cartItemId:int}")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<CartItemDTO>.Fail("Invalid request data"));
                }

                var userId = await GetCurrentUserIdAsync();
                var cartItem = await _cartService.UpdateCartItemAsync(userId, cartItemId, updateDto);
                return Ok(ApiResponse<CartItemDTO>.Ok(cartItem, "Cart item updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<CartItemDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item");
                return StatusCode(500, ApiResponse<CartItemDTO>.Fail("An error occurred while updating cart item"));
            }
        }

        [HttpDelete("items/{cartItemId:int}")]
        public async Task<IActionResult> RemoveCartItem(int cartItemId)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var success = await _cartService.RemoveCartItemAsync(userId, cartItemId);
                return Ok(ApiResponse<bool>.Ok(success, "Cart item removed successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item");
                return StatusCode(500, ApiResponse<bool>.Fail("An error occurred while removing cart item"));
            }
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var success = await _cartService.ClearCartAsync(userId);
                return Ok(ApiResponse<bool>.Ok(success, "Cart cleared successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, ApiResponse<bool>.Fail("An error occurred while clearing cart"));
            }
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userPublicId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            // Look up the user by PublicId to get the integer Id
            var user = await _userRepository.GetByPublicIdAsync(userPublicId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            return user.Id;
        }
    }
}
