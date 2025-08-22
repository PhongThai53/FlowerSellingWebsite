using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Order;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowerSellingWebsite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("{customerId?}/list")]
        public async Task<PagedResult<OrderDTO>> GetListOrderAsync([FromBody] UrlQueryParams queryParams, int? customerId = null)
        {
            return await _orderService.GetOrderHistoryAsync(queryParams, customerId);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            
            return order == null ? NotFound() : Ok(order);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequestDTO checkoutRequest)
        {
            try
            {
                // Get customer ID from JWT token (same as CartController)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userPublicId))
                {
                    return Unauthorized("Invalid user token");
                }

                // Get the actual user ID from the public ID
                var user = await _orderService.GetUserByPublicIdAsync(userPublicId);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                // Process checkout
                var result = await _orderService.ProcessCheckoutAsync(checkoutRequest, user.Id);

                if (result.Succeeded)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CheckoutResponseDTO
                {
                    Succeeded = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }
    }
}
