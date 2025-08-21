using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Product;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowerSellingWebsite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int categoryId = 0,
            [FromQuery] int min = 0,
            [FromQuery] int max = 500000,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool asc = true,
            CancellationToken cancellationToken = default)
        {

            var result = await _productService.GetPagedProductsAsync(
                pageNumber, pageSize, categoryId, min, max, search, sortBy, asc, cancellationToken);

            var data = new
            {
                items = result.Items,
                totalPages = result.TotalPages,
                totalCount = result.TotalCount,
                min = result.Min,
                max = result.DbMax,
                dbmax = result.DbMax
            };

            return Ok(ApiResponse<object>.Ok(data, "Products retrieved successfully"));
        }



        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return Ok(ApiResponse<object>.Ok(product!, "Product retrieved successfully"));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDTO dto)
        {
            var created = await _productService.CreateProductAsync(dto);
            return Ok(ApiResponse<CreateProductDTO>.Ok(created!));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateProductDTO dto, CancellationToken cancellationToken = default)
        {
            var updated = await _productService.UpdateProductAsync(id, dto, cancellationToken);
            return Ok(ApiResponse<UpdateProductDTO>.Ok(updated!));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var deleted = await _productService.DeleteProductAsync(id, cancellationToken);
            return Ok(ApiResponse<bool>.Ok(deleted));
        }
    }
}
