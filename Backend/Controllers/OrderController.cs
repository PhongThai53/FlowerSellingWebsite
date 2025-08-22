using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Order;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowerSellingWebsite.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("list")]
        public async Task<PagedResult<OrderDTO>> GetListOrderAsync(
    [FromBody] UrlQueryParams queryParams)
        {
            return await _orderService.GetOrderHistoryAsync(queryParams, null);
        }

        [HttpPost("{customerId:int}/list")]
        public async Task<PagedResult<OrderDTO>> GetListOrderByCustomerAsync(
            [FromBody] UrlQueryParams queryParams,
            int customerId)
        {
            return await _orderService.GetOrderHistoryAsync(queryParams, customerId);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            
            return order == null ? NotFound() : Ok(order);
        }
    }
}
