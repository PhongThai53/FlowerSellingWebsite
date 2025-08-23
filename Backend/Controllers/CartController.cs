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
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, IUserRepository userRepository, ICartRepository cartRepository, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _userRepository = userRepository;
            _cartRepository = cartRepository;
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Authentication error in GetCartItems: {Message}", ex.Message);
                return Unauthorized(ApiResponse<PagedCartResultDTO>.Fail("Authentication failed. Please log in again."));
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Authentication error in GetCartSummary: {Message}", ex.Message);
                return Unauthorized(ApiResponse<CartSummaryDTO>.Fail("Authentication failed. Please log in again."));
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Authentication error in GetCartItemsCount: {Message}", ex.Message);
                return Unauthorized(ApiResponse<int>.Fail("Authentication failed. Please log in again."));
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Authentication error in ClearCart: {Message}", ex.Message);
                return Unauthorized(ApiResponse<bool>.Fail("Authentication failed. Please log in again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, ApiResponse<bool>.Fail("An error occurred while clearing cart"));
            }
        }

        [HttpGet("debug-token")]
        public async Task<IActionResult> DebugToken()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value;

                _logger.LogInformation("JWT Token Debug - UserId: {UserId}, Email: {Email}, Name: {Name}", 
                    userIdClaim, emailClaim, nameClaim);

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userPublicId))
                {
                    return BadRequest(ApiResponse<object>.Fail("Invalid JWT token format"));
                }

                // Try to find the user
                var user = await _userRepository.GetByPublicIdAsync(userPublicId);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.Fail($"User not found for PublicId: {userPublicId}"));
                }

                return Ok(ApiResponse<object>.Ok(new
                {
                    JwtPublicId = userPublicId,
                    DatabaseUserId = user.Id,
                    DatabasePublicId = user.PublicId,
                    Email = user.Email,
                    Username = user.UserName,
                    IsDeleted = user.IsDeleted,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }, "Token debug information"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in debug token endpoint");
                return StatusCode(500, ApiResponse<object>.Fail($"Debug error: {ex.Message}"));
            }
        }

        [HttpPost("recover")]
        public async Task<IActionResult> RecoverCart()
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                
                // Try to ensure user has an active cart
                var cart = await _cartService.EnsureActiveCartAsync(userId);
                
                return Ok(ApiResponse<CartDTO>.Ok(cart, "Cart recovered successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Authentication error in RecoverCart: {Message}", ex.Message);
                return Unauthorized(ApiResponse<CartDTO>.Fail("Authentication failed. Please log in again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recovering cart");
                return StatusCode(500, ApiResponse<CartDTO>.Fail("An error occurred while recovering cart"));
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetCartStatus()
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
                
                var status = new
                {
                    HasActiveCart = cart != null,
                    CartId = cart?.Id ?? 0,
                    IsCheckedOut = cart?.IsCheckedOut ?? false,
                    IsDeleted = cart?.IsDeleted ?? false,
                    UserId = userId
                };
                
                return Ok(ApiResponse<object>.Ok(status, "Cart status retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Authentication error in GetCartStatus: {Message}", ex.Message);
                return Unauthorized(ApiResponse<object>.Fail("Authentication failed. Please log in again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart status");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while getting cart status"));
            }
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userPublicId))
                {
                    _logger.LogWarning("Invalid or missing user ID claim in JWT token");
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                _logger.LogInformation("Looking up user with PublicId: {PublicId}", userPublicId);

                // Look up the user by PublicId to get the integer Id
                var user = await _userRepository.GetByPublicIdAsync(userPublicId);
                if (user == null)
                {
                    _logger.LogError("User not found in database for PublicId: {PublicId}", userPublicId);
                    throw new UnauthorizedAccessException($"User not found for PublicId: {userPublicId}");
                }

                _logger.LogInformation("Successfully found user: {UserId} with PublicId: {PublicId}", user.Id, userPublicId);
                return user.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCurrentUserIdAsync");
                throw;
            }
        }
    }
}
