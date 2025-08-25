using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Product;
using FlowerSellingWebsite.Services.Interfaces;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FlowerSellingWebsite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ProductController> _logger;
        
        public ProductController(IProductService productService, IWebHostEnvironment env, ILogger<ProductController> logger)
        {
            _productService = productService;
            _env = env;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int categoryId = 0,
            [FromQuery] int min = 0,
            [FromQuery] int max = 10000000,
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

        [HttpGet("check-availability/{productId:int}")]
        public async Task<IActionResult> CheckProductAvailability(int productId, int quantity = 1)
        {
            try
            {
                var availability = await _productService.CheckProductAvailabilityAsync(productId, quantity);
                return Ok(ApiResponse<ProductAvailabilityDTO>.Ok(availability, "Product availability checked successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product availability for product {ProductId}", productId);
                return StatusCode(500, ApiResponse<ProductAvailabilityDTO>.Fail("An error occurred while checking product availability"));
            }
        }

        [HttpGet("debug/{productId:int}")]
        public async Task<IActionResult> DebugProduct(int productId)
        {
            try
            {
                _logger.LogInformation("Debugging product {ProductId}", productId);
                
                // Lấy thông tin sản phẩm
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return Ok(ApiResponse<object>.Fail($"Product {productId} not found"));
                }

                _logger.LogInformation("Product found: {ProductName}", product.Name);

                // Lấy công thức hoa trực tiếp từ repository
                var productFlowersRepo = HttpContext.RequestServices.GetRequiredService<IProductFlowersRepository>();
                var flowerRequirements = await productFlowersRepo.GetFlowerRequirementsForProductAsync(productId);
                
                _logger.LogInformation("Found {FlowerCount} flower requirements", flowerRequirements?.Count() ?? 0);
                
                // Lấy thông tin supplier listings trực tiếp từ repository
                var supplierListingsRepo = HttpContext.RequestServices.GetRequiredService<ISupplierListingsRepository>();
                var supplierListings = new List<object>();
                
                if (flowerRequirements != null && flowerRequirements.Any())
                {
                    foreach (var flowerReq in flowerRequirements)
                    {
                        _logger.LogInformation("Checking flower {FlowerId} ({FlowerName})", flowerReq.FlowerId, flowerReq.Flower?.Name);
                        
                        // Lấy TẤT CẢ supplier listings cho flower này (không filter AvailableQuantity)
                        var allSupplierListings = await supplierListingsRepo.GetByFlowerIdAsync(flowerReq.FlowerId);
                        _logger.LogInformation("Found {TotalSupplierCount} total suppliers for flower {FlowerId}", allSupplierListings?.Count() ?? 0, flowerReq.FlowerId);
                        
                        // Lấy chỉ những supplier có AvailableQuantity > 0
                        var availableFlowers = await supplierListingsRepo.GetAvailableFlowersByFlowerIdAsync(flowerReq.FlowerId);
                        _logger.LogInformation("Found {AvailableSupplierCount} available suppliers for flower {FlowerId}", availableFlowers?.Count() ?? 0, flowerReq.FlowerId);
                        
                        supplierListings.Add(new
                        {
                            FlowerId = flowerReq.FlowerId,
                            FlowerName = flowerReq.Flower?.Name,
                            QuantityNeeded = flowerReq.QuantityNeeded,
                            TotalSuppliers = allSupplierListings?.Count() ?? 0,
                            AvailableSuppliers = availableFlowers?.Count() ?? 0,
                            AllSupplierListings = allSupplierListings?.Select(sl => new
                            {
                                SupplierId = sl.SupplierId,
                                SupplierName = sl.Supplier?.SupplierName,
                                AvailableQuantity = sl.AvailableQuantity,
                                Status = sl.Status,
                                UnitPrice = sl.UnitPrice
                            }).ToList(),
                            AvailableSupplierListings = availableFlowers?.Select(sl => new
                            {
                                SupplierId = sl.SupplierId,
                                SupplierName = sl.Supplier?.SupplierName,
                                AvailableQuantity = sl.AvailableQuantity,
                                Status = sl.Status,
                                UnitPrice = sl.UnitPrice
                            }).ToList()
                        });
                    }
                }

                var debugInfo = new
                {
                    Product = new
                    {
                        Id = product.Id,
                        Name = product.Name
                    },
                    FlowerRequirements = flowerRequirements?.Select(fr => new
                    {
                        FlowerId = fr.FlowerId,
                        FlowerName = fr.Flower?.Name,
                        QuantityNeeded = fr.QuantityNeeded
                    }).ToList(),
                    SupplierListings = supplierListings
                };

                return Ok(ApiResponse<object>.Ok(debugInfo, "Debug information retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error debugging product {ProductId}", productId);
                return StatusCode(500, ApiResponse<object>.Fail($"Debug error: {ex.Message}"));
            }
        }
    }
}
