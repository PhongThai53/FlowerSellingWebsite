using AutoMapper;
using FlowerSellingWebsite.Models.DTOs.Product;
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
    }
}
