using AutoMapper;
using FlowerSellingWebsite.Exceptions;
using FlowerSellingWebsite.Models.DTOs.Product;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductPhotoRepository _productPhotoRepository;
        private readonly IMapper _mapper;

        public ProductService(
            IProductRepository productRepository,
            IProductPhotoRepository productPhotoRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _productPhotoRepository = productPhotoRepository;
            _mapper = mapper;
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

        //public async Task<bool> ReduceStockForOrderAsync(List<(int ProductId, int Quantity)> orderItems, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        foreach (var (productId, quantity) in orderItems)
        //        {
        //            var success = await _productRepository.ReduceProductStockAsync(productId, quantity, cancellationToken);
        //            if (!success)
        //            {
        //                // Log the failure but continue with other products
        //                Console.WriteLine($"Failed to reduce stock for product {productId} by {quantity}");
        //                return false;
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error reducing stock for order: {ex.Message}");
        //        return false;
        //    }
        //}
    }
}
