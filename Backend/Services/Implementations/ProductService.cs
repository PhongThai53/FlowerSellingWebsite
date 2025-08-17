using AutoMapper;
using FlowerSellingWebsite.Exceptions;
using FlowerSellingWebsite.Models.DTOs.Product;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class ProductService : IProductService
    {
        public readonly IProductRepository _productRepository;
        public readonly IMapper _mapper;
        public ProductService(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<ProductDTO> Items, int TotalPages, int TotalCount)> GetPagedProductsAsync(
            int pageNumber,
            int pageSize,
            int categoryId,
            string? search,
            string? sortBy,
            bool asc = true,
            CancellationToken cancellationToken = default)
        {
            // Validation 
            if (pageNumber < 0) pageNumber = 1;
            if (pageSize < 0 || pageSize > 30) pageSize = 10;

            // Get Data
            var (items, totalPages, totalCount) = await _productRepository.GetPagedProductsAsync(
            pageNumber,
            pageSize,
            categoryId,
            search,
            sortBy,
            asc,
            cancellationToken);

            // Map DTO
            var dtoItems = _mapper.Map<IEnumerable<ProductDTO>>(items);

            return (dtoItems, totalPages, totalCount);
        }

        public async Task<ProductDTO?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<UpdateProductDTO?> UpdateProductAsync(int id, UpdateProductDTO dto, CancellationToken cancellationToken = default)
        {
            var existing = await _productRepository.GetProductByIdAsync(id);

            // Validation
            if (existing == null)
                throw new NotFoundException("Product not found.");
            if (dto.Name != null && string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Product name cannot be empty.");
            if (dto.Price.HasValue && dto.Price < 0)
                throw new ValidationException("Price cannot be negative.");
            if (dto.CategoryId.HasValue && dto.CategoryId <= 0)
                throw new ValidationException("CategoryId must be positive.");

            _mapper.Map(dto, existing);
            var updated = await _productRepository.UpdateProductAsync(existing, cancellationToken);
            return _mapper.Map<UpdateProductDTO?>(updated);
        }
        public async Task<CreateProductDTO?> CreateProductAsync(CreateProductDTO dto, CancellationToken cancellationToken = default)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Product name is required.");
            if (dto.Price.HasValue && dto.Price < 0)
                throw new ValidationException("Price cannot be negative.");
            if (dto.CategoryId <= 0)
                throw new ValidationException("CategoryId must be positive.");

            var product = _mapper.Map<Products>(dto);
            var created = await _productRepository.CreateProductAsync(product, cancellationToken);
            return _mapper.Map<CreateProductDTO?>(created);
        }

        public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _productRepository.GetProductByIdAsync(id);
            if (existing == null) return false;

            return await _productRepository.DeleteProductAsync(id, cancellationToken);
        }
    }
}
