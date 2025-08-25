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
        private readonly IProductFlowersRepository _productFlowersRepository;
        private readonly IMapper _mapper;

        public ProductService(
            IProductRepository productRepository,
            IProductPhotoRepository productPhotoRepository,
            IProductFlowersRepository productFlowersRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _productPhotoRepository = productPhotoRepository;
            _productFlowersRepository = productFlowersRepository;
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
            try
            {
                pageNumber = pageNumber < 1 ? 1 : pageNumber;
                pageSize = pageSize <= 0 || pageSize > 30 ? 10 : pageSize;

                var result = await _productRepository.GetPagedProductsAsync(
                    pageNumber,
                    pageSize,
                    categoryId,
                    min,
                    max,
                    search,
                    sortBy,
                    asc);

                var dtoItems = _mapper.Map<IEnumerable<ProductDTO>>(result.Items);

                return (dtoItems, result.TotalPages, result.TotalCount, result.Min, result.Max, result.DbMax);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetPagedProductsAsync Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<ProductDTO?> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(id);
                return product != null ? _mapper.Map<ProductDTO>(product) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetProductByIdAsync Error - ID {id}: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<UpdateProductDTO?> UpdateProductAsync(int id, UpdateProductDTO dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                ValidateDataAnnotations(dto);

                var existing = await _productRepository.GetProductByIdAsync(id);
                if (existing == null)
                    throw new NotFoundException("Product not found.");

                _mapper.Map(dto, existing);

                if (dto.ProductPhotos != null && dto.ProductPhotos.Any())
                {
                    var photos = _mapper.Map<List<ProductPhotos>>(dto.ProductPhotos);
                    await HandleProductPhotos(photos, id);
                }

                if (dto.ProductFlowers != null && dto.ProductFlowers.Any())
                {
                    var flowers = _mapper.Map<List<ProductFlowers>>(dto.ProductFlowers);
                    await HandleProductFLowers(flowers, id);
                }

                var updated = await _productRepository.UpdateProductAsync(existing);
                return _mapper.Map<UpdateProductDTO>(updated);
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
            try
            {
                if (dto == null)
                    return null;

                ValidateDataAnnotations(dto);

                var product = _mapper.Map<Products>(dto);
                var createdProduct = await _productRepository.CreateProductAsync(product);

                if (dto.ProductPhotos != null && dto.ProductPhotos.Any())
                {
                    var photos = _mapper.Map<List<ProductPhotos>>(dto.ProductPhotos);
                    await HandleProductPhotos(photos, createdProduct.Id);
                }

                if (dto.ProductFlowers != null && dto.ProductFlowers.Any())
                {
                    var flowers = _mapper.Map<List<ProductFlowers>>(dto.ProductFlowers);
                    await HandleProductFLowers(flowers, createdProduct.Id);
                }

                return _mapper.Map<CreateProductDTO>(createdProduct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateProductAsync Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }


        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var existing = await _productRepository.GetProductByIdAsync(id);
                if (existing == null)
                    return false;

                return await _productRepository.DeleteProductAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteProductAsync Error - ID {id}: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private static void ValidateDataAnnotations<T>(T? dto) where T : class
        {
            if (dto == null)
            {
                throw new FlowerSellingWebsite.Exceptions.ValidationException("DTO must not be null.");
            }

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
                photo.Id = 0;
                ValidateDataAnnotations(photo);
                await _productPhotoRepository.createAsync(photo);
            }
        }

        private static void ValidateProductFLowers(List<ProductFlowers>? flowers)
        {
            if (flowers == null || !flowers.Any())
                return;
            foreach (var flower in flowers)
            {
                ValidateDataAnnotations(flower);
            }
        }

        private async Task HandleProductFLowers(List<ProductFlowers> flowers, int productId)
        {
            foreach (var flower in flowers)
            {
                flower.ProductId = productId;
                flower.Id = 0;
                ValidateDataAnnotations(flower);
                await _productFlowersRepository.createAsync(flower);
            }
        }
    }
}
