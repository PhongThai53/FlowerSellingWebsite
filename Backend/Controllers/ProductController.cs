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
        private readonly IWebHostEnvironment _env;
        public ProductController(IProductService productService, IWebHostEnvironment env)
        {
            _productService = productService;
            _env = env;
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
            [FromQuery] bool asc = true)
        {

            var result = await _productService.GetPagedProductsAsync(
                pageNumber, pageSize, categoryId, min, max, search, sortBy, asc);

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
        public async Task<IActionResult> Update(int id, UpdateProductDTO dto)
        {
            var updated = await _productService.UpdateProductAsync(id, dto);
            return Ok(ApiResponse<UpdateProductDTO>.Ok(updated!));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _productService.DeleteProductAsync(id);
            return Ok(ApiResponse<bool>.Ok(deleted));
        }


        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadImage(int id, List<IFormFile> productPhotos)
        {
            if (productPhotos == null || productPhotos.Count == 0)
            {
                return BadRequest("No files uploaded.");
            }

            var uploadPath = Path.Combine(_env.WebRootPath, "Image", "products", id.ToString());

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            for (int i = 0; i < productPhotos.Count; i++)
            {
                var file = productPhotos[i];

                var fileName = i == 0 ? "primary.jpg" : file.FileName;

                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            return Ok(ApiResponse<string>.Ok("Images uploaded successfully"));
        }
    }
}
