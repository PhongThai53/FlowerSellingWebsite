using FlowerSellingWebsite.Models.DTOs.Order;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowerSellingWebsite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated, searchable, and filterable list of orders
        /// </summary>
        /// <param name="filters">Filter parameters including search, pagination, and sorting options</param>
        /// <returns>Paginated list of orders</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<PagedOrdersResultDTO>> GetOrders([FromQuery] OrderFilterDTO filters)
        {
            try
            {
                var result = await _orderService.GetOrdersWithFiltersAsync(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders with filters");
                return StatusCode(500, "An error occurred while retrieving orders");
            }
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Order details</returns>
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<OrderDTO>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                return Ok(order);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Order with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by ID {OrderId}", id);
                return StatusCode(500, "An error occurred while retrieving the order");
            }
        }

        /// <summary>
        /// Get order by public ID
        /// </summary>
        /// <param name="publicId">Order public ID</param>
        /// <returns>Order details</returns>
        [HttpGet("public/{publicId:guid}")]
        [Authorize]
        public async Task<ActionResult<OrderDTO>> GetOrderByPublicId(Guid publicId)
        {
            try
            {
                var order = await _orderService.GetOrderByPublicIdAsync(publicId);
                return Ok(order);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Order with public ID {publicId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by public ID {PublicId}", publicId);
                return StatusCode(500, "An error occurred while retrieving the order");
            }
        }

        /// <summary>
        /// Get orders by customer ID
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>List of orders for the customer</returns>
        [HttpGet("customer/{customerId:int}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersByCustomerId(int customerId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for customer {CustomerId}", customerId);
                return StatusCode(500, "An error occurred while retrieving customer orders");
            }
        }

        /// <summary>
        /// Get orders by supplier ID
        /// </summary>
        /// <param name="supplierId">Supplier ID</param>
        /// <returns>List of orders for the supplier</returns>
        [HttpGet("supplier/{supplierId:int}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersBySupplier(int supplierId)
        {
            try
            {
                var orders = await _orderService.GetOrdersBySupplierIdAsync(supplierId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for supplier {SupplierId}", supplierId);
                return StatusCode(500, "An error occurred while retrieving supplier orders");
            }
        }

        /// <summary>
        /// Get orders by status
        /// </summary>
        /// <param name="status">Order status</param>
        /// <returns>List of orders with the specified status</returns>
        [HttpGet("status/{status}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersByStatus(string status)
        {
            try
            {
                var orders = await _orderService.GetOrdersByStatusAsync(status);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders with status {Status}", status);
                return StatusCode(500, "An error occurred while retrieving orders by status");
            }
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        /// <param name="createOrderDTO">Order creation data</param>
        /// <returns>Created order details</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<OrderDTO>> CreateOrder(CreateOrderDTO createOrderDTO)
        {
            try
            {
                // Get the current user ID from the claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest("User ID not found or invalid");
                }

                var order = await _orderService.CreateOrderAsync(createOrderDTO, userId);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, "An error occurred while creating the order");
            }
        }

        /// <summary>
        /// Update an existing order
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="updateOrderDTO">Order update data</param>
        /// <returns>Success status</returns>
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<ActionResult> UpdateOrder(int id, UpdateOrderDTO updateOrderDTO)
        {
            try
            {
                var success = await _orderService.UpdateOrderAsync(id, updateOrderDTO);
                if (!success)
                {
                    return NotFound($"Order with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", id);
                return StatusCode(500, "An error occurred while updating the order");
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="status">New status</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id:int}/status")]
        [Authorize]
        public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            try
            {
                var success = await _orderService.UpdateOrderStatusAsync(id, status);
                if (!success)
                {
                    return NotFound($"Order with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for order {OrderId}", id);
                return StatusCode(500, "An error occurred while updating the order status");
            }
        }

        /// <summary>
        /// Delete an order
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            try
            {
                var success = await _orderService.DeleteOrderAsync(id);
                if (!success)
                {
                    return NotFound($"Order with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order {OrderId}", id);
                return StatusCode(500, "An error occurred while deleting the order");
            }
        }

        /// <summary>
        /// Get order detail by ID
        /// </summary>
        /// <param name="id">Order detail ID</param>
        /// <returns>Order detail</returns>
        [HttpGet("detail/{id:int}")]
        [Authorize]
        public async Task<ActionResult<OrderDetailDTO>> GetOrderDetail(int id)
        {
            try
            {
                var orderDetail = await _orderService.GetOrderDetailByIdAsync(id);
                return Ok(orderDetail);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Order detail with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order detail {OrderDetailId}", id);
                return StatusCode(500, "An error occurred while retrieving the order detail");
            }
        }

        /// <summary>
        /// Add a new detail to an existing order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="createOrderDetailDTO">Order detail creation data</param>
        /// <returns>Created order detail</returns>
        [HttpPost("{orderId:int}/detail")]
        [Authorize]
        public async Task<ActionResult<OrderDetailDTO>> AddOrderDetail(int orderId, CreateOrderDetailDTO createOrderDetailDTO)
        {
            try
            {
                var orderDetail = await _orderService.AddOrderDetailAsync(orderId, createOrderDetailDTO);
                return CreatedAtAction(nameof(GetOrderDetail), new { id = orderDetail.Id }, orderDetail);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding detail to order {OrderId}", orderId);
                return StatusCode(500, "An error occurred while adding the order detail");
            }
        }

        /// <summary>
        /// Update an order detail
        /// </summary>
        /// <param name="id">Order detail ID</param>
        /// <param name="updateOrderDetailDTO">Order detail update data</param>
        /// <returns>Success status</returns>
        [HttpPut("detail/{id:int}")]
        [Authorize]
        public async Task<ActionResult> UpdateOrderDetail(int id, UpdateOrderDetailDTO updateOrderDetailDTO)
        {
            try
            {
                var success = await _orderService.UpdateOrderDetailAsync(id, updateOrderDetailDTO);
                if (!success)
                {
                    return NotFound($"Order detail with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order detail {OrderDetailId}", id);
                return StatusCode(500, "An error occurred while updating the order detail");
            }
        }

        /// <summary>
        /// Delete an order detail
        /// </summary>
        /// <param name="id">Order detail ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("detail/{id:int}")]
        [Authorize]
        public async Task<ActionResult> DeleteOrderDetail(int id)
        {
            try
            {
                var success = await _orderService.DeleteOrderDetailAsync(id);
                if (!success)
                {
                    return NotFound($"Order detail with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order detail {OrderDetailId}", id);
                return StatusCode(500, "An error occurred while deleting the order detail");
            }
        }
    }
}
