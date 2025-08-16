using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowerSellingWebsite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// Get paged products with search and sorting
        [HttpGet]
        public async Task<IActionResult> GetPagedProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool asc = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.GetPagedProductsAsync(
                    pageNumber, pageSize, search, sortBy, asc, cancellationToken);

                var data = new
                {
                    items = result.Items,
                    totalPages = result.TotalPages,
                    totalCount = result.TotalCount
                };

                return Ok(ApiResponse<object>.Ok(data, "Products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving paged products.");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Product is not found."));
                }
                return Ok(ApiResponse<object>.Ok(product, "Product retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the product.");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while processing your request."));
            }
        }
    }
}
