using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Product;
using FlowerSellingWebsite.Models.DTOs.ProductPhoto;
using FlowerSellingWebsite.Models.DTOs.ProductFlower;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Services.Interfaces;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FlowerSelling.Data.FlowerSellingWebsite.Data;
using Microsoft.EntityFrameworkCore;

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

        // Admin endpoint to get all products including deleted ones
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllProductsIncludingDeleted(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int categoryId = 0,
            [FromQuery] int min = 0,
            [FromQuery] int max = 10000000,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool asc = true)
        {

            var result = await _productService.GetAllProductsIncludingDeletedAsync(
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

            return Ok(ApiResponse<object>.Ok(data, "All products (including deleted) retrieved successfully"));
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
            return Ok(ApiResponse<bool>.Ok(deleted, "Product deactivated successfully"));
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var activated = await _productService.ActivateProductAsync(id);
            return Ok(ApiResponse<bool>.Ok(activated, "Product activated successfully"));
        }


        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file, bool isPrimary = false)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Check if isPrimary parameter was received correctly
            var isPrimaryFromForm = Request.Form.ContainsKey("isPrimary") ? Request.Form["isPrimary"].ToString() : "false";
            bool isPrimaryParsed = bool.TryParse(isPrimaryFromForm, out bool parsed) ? parsed : false;

            var uploadPath = Path.Combine(_env.WebRootPath, "images", "products", id.ToString());
            
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // For primary image, always save as primary.jpg
            // For additional images, use original filename
            var fileName = isPrimaryParsed ? "primary.jpg" : file.FileName;
            var filePath = Path.Combine(uploadPath, fileName);

            try
            {
                // Delete existing primary.jpg if it exists
                if (isPrimaryParsed && System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // If this is primary image, update the product URL and handle database records
                if (isPrimaryParsed)
                {
                    try
                    {
                        // Delete old primary photo records from database
                        var productPhotosRepo = HttpContext.RequestServices.GetRequiredService<IProductPhotoRepository>();
                        var existingPhotos = await productPhotosRepo.GetPhotosByProductIdAsync(id);
                        var primaryPhotos = existingPhotos.Where(p => p.IsPrimary).ToList();
                        
                        foreach (var photo in primaryPhotos)
                        {
                            await productPhotosRepo.deleteAsync(photo);
                        }

                        // Create new primary photo record
                        var newPrimaryPhoto = new ProductPhotos
                        {
                            ProductId = id,
                            Url = "primary.jpg",
                            IsPrimary = true
                        };
                        await productPhotosRepo.createAsync(newPrimaryPhoto);

                        // Update product URL
                        var product = await _productService.GetProductByIdAsync(id);
                        if (product != null)
                        {
                            var updateDto = new UpdateProductDTO
                            {
                                Name = product.Name ?? "",
                                Description = product.Description,
                                Price = product.Price,
                                CategoryId = product.CategoryId,
                                Url = "primary.jpg"
                            };
                            
                            await _productService.UpdateProductAsync(id, updateDto);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating product URL for product {ProductId}", id);
                        // Don't fail the entire upload, just log the error
                    }
                }

                _logger.LogInformation("Image uploaded successfully for product {ProductId}: {FilePath}", id, filePath);
                return Ok(ApiResponse<string>.Ok(filePath, "Image uploaded successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for product {ProductId}", id);
                return StatusCode(500, ApiResponse<string>.Fail("Failed to upload image"));
            }
        }

        [HttpPost("{id}/photos")]
        public async Task<IActionResult> CreateProductPhotos(int id, [FromBody] List<CreateProductPhotoDTO> photoDtos)
        {
            try
            {
                if (photoDtos == null || !photoDtos.Any())
                {
                    return BadRequest("No photo data provided.");
                }

                // Validate that the product exists
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                // Delete old additional photos (non-primary) from database
                var productPhotosRepo = HttpContext.RequestServices.GetRequiredService<IProductPhotoRepository>();
                var existingPhotos = await productPhotosRepo.GetPhotosByProductIdAsync(id);
                var additionalPhotos = existingPhotos.Where(p => !p.IsPrimary).ToList();
                
                foreach (var photo in additionalPhotos)
                {
                    await productPhotosRepo.deleteAsync(photo);
                }

                // Create new ProductPhotos records
                var productPhotos = photoDtos.Select(dto => new ProductPhotos
                {
                    ProductId = id,
                    Url = dto.Url,
                    IsPrimary = dto.IsPrimary
                }).ToList();

                // Save to database using repository
                foreach (var photo in productPhotos)
                {
                    await productPhotosRepo.createAsync(photo);
                }

                return Ok(ApiResponse<List<ProductPhotos>>.Ok(productPhotos, "Product photos created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product photos for product {ProductId}", id);
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while creating product photos"));
            }
        }

        // Get ProductFlowers records for product recipe
        [HttpGet("{id}/flowers")]
        public async Task<IActionResult> GetProductFlowers(int id)
        {
            try
            {
                _logger.LogInformation("Getting flower recipe for product {ProductId}", id);
                
                var productFlowersRepo = HttpContext.RequestServices.GetRequiredService<IProductFlowersRepository>();
                var flowerRequirements = await productFlowersRepo.GetFlowerRequirementsForProductAsync(id);
                
                _logger.LogInformation("Found {FlowerCount} flower requirements for product {ProductId}", 
                    flowerRequirements?.Count() ?? 0, id);
                
                if (flowerRequirements == null || !flowerRequirements.Any())
                {
                    _logger.LogInformation("No flower recipe found for product {ProductId}", id);
                    return Ok(ApiResponse<List<object>>.Ok(new List<object>(), "No flower recipe found"));
                }

                var flowerDtos = flowerRequirements.Select(fr => new
                {
                    flowerId = fr.FlowerId,
                    quantityNeeded = fr.QuantityNeeded
                }).ToList();

                _logger.LogInformation("Returning {FlowerCount} flower DTOs for product {ProductId}", flowerDtos.Count, id);
                return Ok(ApiResponse<object>.Ok(flowerDtos, "Flower recipe retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flower recipe for product {ProductId}", id);
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while getting flower recipe"));
            }
        }

        // Delete existing ProductFlowers records for product recipe
        [HttpDelete("{id}/flowers")]
        public async Task<IActionResult> DeleteProductFlowers(int id)
        {
            try
            {
                var productFlowersRepo = HttpContext.RequestServices.GetRequiredService<IProductFlowersRepository>();
                
                // Use raw SQL to delete all ProductFlowers for this product
                var context = HttpContext.RequestServices.GetRequiredService<FlowerSellingDbContext>();
                var deletedCount = await context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM ProductFlowers WHERE ProductId = {0}", id);
                
                // Clear change tracker to avoid conflicts with new entities
                context.ChangeTracker.Clear();
                
                _logger.LogInformation("Deleted {DeletedCount} flower recipe records for product {ProductId} and cleared change tracker", deletedCount, id);

                return Ok(ApiResponse<bool>.Ok(true, $"Deleted {deletedCount} existing flower recipe records successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting flower recipe for product {ProductId}", id);
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while deleting flower recipe"));
            }
        }

        // Simple update ProductFlowers records - just DELETE + INSERT
        [HttpPost("{id}/flowers")]
        public async Task<IActionResult> CreateProductFlowers(int id, [FromBody] List<CreateProductFlowerDTO> flowerDtos)
        {
            try
            {
                if (flowerDtos == null || !flowerDtos.Any())
                {
                    return BadRequest("No flower data provided.");
                }

                // Validate that the product exists
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                // Use raw SQL to avoid all EF tracking issues
                var connectionString = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetConnectionString("DefaultConnection");
                
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                await connection.OpenAsync();
                using var transaction = connection.BeginTransaction();
                
                try
                {
                    // Use MERGE statement to UPSERT flower recipe records
                    var mergeSql = @"
                        MERGE ProductFlowers AS target
                        USING (VALUES (@productId, @flowerId, @quantity, @now, @now, 0, @guid)) AS source (ProductId, FlowerId, QuantityNeeded, CreatedAt, UpdatedAt, IsDeleted, PublicId)
                        ON target.ProductId = source.ProductId AND target.FlowerId = source.FlowerId
                        WHEN MATCHED THEN
                            UPDATE SET 
                                QuantityNeeded = source.QuantityNeeded,
                                UpdatedAt = source.UpdatedAt,
                                IsDeleted = source.IsDeleted
                        WHEN NOT MATCHED THEN
                            INSERT (ProductId, FlowerId, QuantityNeeded, CreatedAt, UpdatedAt, IsDeleted, PublicId)
                            VALUES (source.ProductId, source.FlowerId, source.QuantityNeeded, source.CreatedAt, source.UpdatedAt, source.IsDeleted, source.PublicId);
                    ";
                    
                    var updatedCount = 0;
                    var insertedCount = 0;
                    
                    foreach (var dto in flowerDtos)
                    {
                        using var mergeCommand = new Microsoft.Data.SqlClient.SqlCommand(mergeSql, connection, transaction);
                        mergeCommand.Parameters.AddWithValue("@productId", id);
                        mergeCommand.Parameters.AddWithValue("@flowerId", dto.FlowerId);
                        mergeCommand.Parameters.AddWithValue("@quantity", dto.QuantityNeeded);
                        mergeCommand.Parameters.AddWithValue("@now", DateTime.UtcNow);
                        mergeCommand.Parameters.AddWithValue("@guid", Guid.NewGuid());
                        
                        var rowsAffected = await mergeCommand.ExecuteNonQueryAsync();
                        
                        if (rowsAffected == 1)
                        {
                            // Check if it was an UPDATE or INSERT by looking at existing record
                            var checkSql = "SELECT COUNT(*) FROM ProductFlowers WHERE ProductId = @productId AND FlowerId = @flowerId AND IsDeleted = 0";
                            using var checkCommand = new Microsoft.Data.SqlClient.SqlCommand(checkSql, connection, transaction);
                            checkCommand.Parameters.AddWithValue("@productId", id);
                            checkCommand.Parameters.AddWithValue("@flowerId", dto.FlowerId);
                            var existingCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                            
                            if (existingCount > 0)
                            {
                                updatedCount++;
                            }
                            else
                            {
                                insertedCount++;
                            }
                        }
                        else
                        {
                            insertedCount++;
                        }
                    }
                    
                    _logger.LogInformation("Upserted flower recipe for product {ProductId}: updated {UpdatedCount}, inserted {InsertedCount}", 
                        id, updatedCount, insertedCount);
                    
                    transaction.Commit();
                    
                    _logger.LogInformation("Successfully updated flower recipe for product {ProductId}: updated {UpdatedCount}, inserted {InsertedCount}", 
                        id, updatedCount, insertedCount);
                    
                    return Ok(ApiResponse<object>.Ok(new { updatedCount, insertedCount }, "Flower recipe updated successfully"));
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating flower recipe for product {ProductId}", id);
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while updating flower recipe"));
            }
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
