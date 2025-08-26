using AutoMapper;
using FlowerSellingWebsite.Exceptions;
using FlowerSellingWebsite.Models.DTOs.Product;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductPhotoRepository _productPhotoRepository;
        private readonly IProductFlowersRepository _productFlowersRepository;
        private readonly ISupplierListingsRepository _supplierListingsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            IProductPhotoRepository productPhotoRepository,
            IProductFlowersRepository productFlowersRepository,
            ISupplierListingsRepository supplierListingsRepository,
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _productPhotoRepository = productPhotoRepository;
            _productFlowersRepository = productFlowersRepository;
            _supplierListingsRepository = supplierListingsRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<(IEnumerable<ProductDTO> Items, int TotalPages, int TotalCount, int Min, int Max, int DbMax)> GetPagedProductsAsync(
            int pageNumber,
            int pageSize,
            int categoryId,
            int min,
            int max,
            string? search,
            string? sortBy,
            bool asc = true)
        {
            // Validation 
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize <= 0 || pageSize > 30 ? 10 : pageSize;

            // Get Data
            var result = await _productRepository.GetPagedProductsAsync(
                pageNumber,
                pageSize,
                categoryId,
                min,
                max,
                search,
                sortBy,
                asc);

            // Map DTO
            var dtoItems = _mapper.Map<IEnumerable<ProductDTO>>(result.Items);

            return (dtoItems, result.TotalPages, result.TotalCount, result.Min, result.Max, result.DbMax);
        }

        public async Task<ProductDTO?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            return product != null ? _mapper.Map<ProductDTO>(product) : null;
        }

        public async Task<UpdateProductDTO?> UpdateProductAsync(int id, UpdateProductDTO dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                ValidateDataAnnotations(dto);
                ValidateProductPhotos(dto.ProductPhotos);

                var existing = await _productRepository.GetProductByIdAsync(id);
                if (existing == null)
                    throw new NotFoundException("Product not found.");

                _mapper.Map(dto, existing);

                if (dto.ProductPhotos != null && dto.ProductPhotos.Any())
                {
                    await HandleProductPhotos(dto.ProductPhotos, id);
                }

                var updated = await _productRepository.UpdateProductAsync(existing);
                return _mapper.Map<UpdateProductDTO?>(updated);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateProduct Error - ID {id}: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<CreateProductDTO?> CreateProductAsync(CreateProductDTO dto)
        {
            if (dto == null)
                return null;

            ValidateDataAnnotations(dto);
            ValidateProductPhotos(dto.ProductPhotos);

            var product = _mapper.Map<Products>(dto);
            var createdProduct = await _productRepository.CreateProductAsync(product);

            if (dto.ProductPhotos != null && dto.ProductPhotos.Any())
            {
                await HandleProductPhotos(dto.ProductPhotos, createdProduct.Id);
            }

            // Map Entity back to DTO
            var result = _mapper.Map<CreateProductDTO>(createdProduct);
            result.ProductPhotos = dto.ProductPhotos ?? new List<ProductPhotos>();

            return result;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var existing = await _productRepository.GetProductByIdAsync(id);
            if (existing == null)
                return false;

            return await _productRepository.DeleteProductAsync(id);
        }

        public async Task<ProductAvailabilityDTO> CheckProductAvailabilityAsync(int productId, int quantity)
        {
            try
            {
                _logger.LogInformation("Checking product availability for product {ProductId} with quantity {Quantity}", productId, quantity);
                
                // Lấy thông tin sản phẩm và công thức hoa
                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found", productId);
                    return new ProductAvailabilityDTO
                    {
                        IsAvailable = false,
                        ProductId = productId,
                        Message = "Sản phẩm không tồn tại"
                    };
                }

                _logger.LogInformation("Product found: {ProductName}", product.Name);

                var flowerRequirements = await _productFlowersRepository.GetFlowerRequirementsForProductAsync(productId);
                if (flowerRequirements == null || !flowerRequirements.Any())
                {
                    _logger.LogWarning("Product {ProductId} has no flower requirements", productId);
                    return new ProductAvailabilityDTO
                    {
                        IsAvailable = false,
                        ProductId = productId,
                        ProductName = product.Name,
                        RequestedQuantity = quantity,
                        Message = "Sản phẩm không có công thức hoa"
                    };
                }

                _logger.LogInformation("Found {FlowerCount} flower requirements for product {ProductId}", flowerRequirements.Count(), productId);

                var result = new ProductAvailabilityDTO
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    RequestedQuantity = quantity,
                    FlowerRequirements = new List<Models.DTOs.Product.FlowerRequirementInfo>()
                };

                int maxAvailableQuantity = int.MaxValue;
                bool isAvailable = true;

                foreach (var flowerReq in flowerRequirements)
                {
                    var totalFlowersNeeded = flowerReq.QuantityNeeded * quantity;
                    _logger.LogInformation("Flower {FlowerId} ({FlowerName}) needs {TotalNeeded} for {Quantity} products", 
                        flowerReq.FlowerId, flowerReq.Flower?.Name, totalFlowersNeeded, quantity);
                    
                    // Console logging để debug
                    Console.WriteLine($"=== DEBUG FLOWER {flowerReq.FlowerId} ===");
                    Console.WriteLine($"Flower: {flowerReq.Flower?.Name}");
                    Console.WriteLine($"Quantity needed per product: {flowerReq.QuantityNeeded}");
                    Console.WriteLine($"Total needed for {quantity} products: {totalFlowersNeeded}");
                    
                    // SO SÁNH 2 METHOD
                    Console.WriteLine($"\n=== COMPARING TWO METHODS ===");
                    
                    // Method 1: GetByFlowerIdAsync (không filter)
                    var allSuppliers = await _supplierListingsRepository.GetByFlowerIdAsync(flowerReq.FlowerId);
                    Console.WriteLine($"Method 1 - GetByFlowerIdAsync (no filter): {allSuppliers?.Count() ?? 0} suppliers");
                    if (allSuppliers != null && allSuppliers.Any())
                    {
                        foreach (var supplier in allSuppliers)
                        {
                            Console.WriteLine($"  Supplier {supplier.SupplierId} ({supplier.Supplier?.SupplierName}): AvailableQty={supplier.AvailableQuantity}, Status={supplier.Status}");
                        }
                    }
                    
                    // Method 2: GetAvailableFlowersByFlowerIdAsync (có filter)
                    var availableFlowers = await _supplierListingsRepository.GetAvailableFlowersByFlowerIdAsync(flowerReq.FlowerId);
                    Console.WriteLine($"Method 2 - GetAvailableFlowersByFlowerIdAsync (with filter): {availableFlowers?.Count() ?? 0} suppliers");
                    if (availableFlowers != null && availableFlowers.Any())
                    {
                        foreach (var supplier in availableFlowers)
                        {
                            Console.WriteLine($"  Supplier {supplier.SupplierId} ({supplier.Supplier?.SupplierName}): AvailableQty={supplier.AvailableQuantity}, Status={supplier.Status}");
                        }
                    }
                    
                    // Kiểm tra tại sao filter không hoạt động
                    var availableCount = allSuppliers?.Count(sl => sl.AvailableQuantity > 0) ?? 0;
                    var availableStatusCount = allSuppliers?.Count(sl => sl.Status == "available") ?? 0;
                    var bothConditionsCount = allSuppliers?.Count(sl => sl.AvailableQuantity > 0 && sl.Status == "available") ?? 0;
                    
                    Console.WriteLine($"\n=== FILTER ANALYSIS ===");
                    Console.WriteLine($"Suppliers with AvailableQuantity > 0: {availableCount}");
                    Console.WriteLine($"Suppliers with Status = 'available': {availableStatusCount}");
                    Console.WriteLine($"Suppliers with BOTH conditions: {bothConditionsCount}");
                    
                    // Kiểm tra từng supplier tại sao bị filter out
                    if (allSuppliers != null)
                    {
                        foreach (var supplier in allSuppliers)
                        {
                            var availableQtyOk = supplier.AvailableQuantity > 0;
                            var statusOk = supplier.Status == "available";
                            var bothOk = availableQtyOk && statusOk;
                            
                            Console.WriteLine($"  Supplier {supplier.SupplierId}: AvailableQty>0={availableQtyOk}, Status='available'={statusOk}, BOTH={bothOk}");
                        }
                    }
                    
                    // SỬ DỤNG KẾT QUẢ TỪ METHOD 1 (không filter) để tính toán
                    int availableQuantity = 0;
                    if (allSuppliers != null && allSuppliers.Any())
                    {
                        availableQuantity = allSuppliers.Sum(sl => sl.AvailableQuantity);
                        Console.WriteLine($"\n=== USING METHOD 1 RESULTS ===");
                        Console.WriteLine($"Total available from Method 1: {availableQuantity}");
                    }
                    else
                    {
                        Console.WriteLine($"\n❌ NO SUPPLIERS AT ALL FOUND!");
                    }

                    var flowerInfo = new Models.DTOs.Product.FlowerRequirementInfo
                    {
                        FlowerId = flowerReq.FlowerId,
                        FlowerName = flowerReq.Flower?.Name ?? "Unknown",
                        QuantityNeeded = flowerReq.QuantityNeeded,
                        TotalQuantityNeeded = totalFlowersNeeded,
                        AvailableQuantity = availableQuantity,
                        IsAvailable = availableQuantity >= totalFlowersNeeded,
                        Shortage = Math.Max(0, totalFlowersNeeded - availableQuantity)
                    };

                    result.FlowerRequirements.Add(flowerInfo);

                    if (!flowerInfo.IsAvailable)
                    {
                        isAvailable = false;
                        _logger.LogWarning("Flower {FlowerId} ({FlowerName}) is not available. Need: {TotalNeeded}, Available: {AvailableQuantity}", 
                            flowerReq.FlowerId, flowerReq.Flower?.Name, totalFlowersNeeded, availableQuantity);
                        Console.WriteLine($"❌ FLOWER NOT AVAILABLE: Need {totalFlowersNeeded}, Available {availableQuantity}");
                    }
                    else
                    {
                        Console.WriteLine($"✅ FLOWER AVAILABLE: Need {totalFlowersNeeded}, Available {availableQuantity}");
                    }

                    // Tính số lượng tối đa có thể làm được
                    int maxQuantityForThisFlower = availableQuantity / flowerReq.QuantityNeeded;
                    maxAvailableQuantity = Math.Min(maxAvailableQuantity, maxQuantityForThisFlower);
                    _logger.LogInformation("Max quantity for flower {FlowerId}: {MaxQuantity}", flowerReq.FlowerId, maxQuantityForThisFlower);
                    Console.WriteLine($"Max quantity for this flower: {maxQuantityForThisFlower}");
                }

                result.IsAvailable = isAvailable;
                result.MaxAvailableQuantity = maxAvailableQuantity;

                if (isAvailable)
                {
                    result.Message = $"Có thể làm được {quantity} sản phẩm";
                    _logger.LogInformation("Product {ProductId} is available for quantity {Quantity}", productId, quantity);
                }
                else
                {
                    result.Message = "Hết hoa để làm sản phẩm";
                    _logger.LogWarning("Product {ProductId} is not available for quantity {Quantity}. Max available: {MaxAvailable}", 
                        productId, quantity, maxAvailableQuantity);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product availability for product {ProductId}", productId);
                return new ProductAvailabilityDTO
                {
                    IsAvailable = false,
                    ProductId = productId,
                    Message = $"Lỗi kiểm tra khả năng cung cấp: {ex.Message}"
                };
            }
        }

        // Private Helper Methods
        private static void ValidateDataAnnotations<T>(T dto) where T : class
        {
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(dto, context, results, true))
            {
                var errorMessage = string.Join("; ", results.Select(r => r.ErrorMessage));
                throw new System.ComponentModel.DataAnnotations.ValidationException(errorMessage);
            }
        }

        private static void ValidateProductPhotos(List<ProductPhotos>? photos)
        {
            if (photos == null || !photos.Any())
                return;

            var primaryCount = photos.Count(p => p.IsPrimary);

            if (primaryCount > 1)
                throw new FlowerSellingWebsite.Exceptions.ValidationException("Only one photo can be set as primary.");

            if (primaryCount == 0)
                throw new FlowerSellingWebsite.Exceptions.ValidationException("At least one photo must be set as primary.");

            foreach (var photo in photos)
            {
                ValidateDataAnnotations(photo);
            }
        }

        private async Task HandleProductPhotos(List<ProductPhotos> photos, int productId)
        {
            foreach (var photo in photos)
            {
                photo.ProductId = productId;
                photo.Id = 0; // Reset ID for new entity
                ValidateDataAnnotations(photo);
                await _productPhotoRepository.createAsync(photo);
            }
        }

    }
}
