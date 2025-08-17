using FlowerSellingWebsite.Models.DTOs.Product;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IProductService
    {
        public Task<(IEnumerable<ProductDTO> Items, int TotalPages, int TotalCount)> GetPagedProductsAsync(
           int pageNumber,
           int pageSize,
           string? search,
           string? sortBy,
           bool asc = true,
           CancellationToken cancellationToken = default);
        Task<ProductDTO?> GetProductByIdAsync(int id);

        Task<CreateProductDTO?> CreateProductAsync(CreateProductDTO dto, CancellationToken cancellationToken = default);

        Task<UpdateProductDTO?> UpdateProductAsync(int id, UpdateProductDTO dto, CancellationToken cancellationToken = default);

        Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
    }
}
