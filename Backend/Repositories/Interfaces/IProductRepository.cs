using FlowerSellingWebsite.Models.Entities;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface IProductRepository
    {
        public Task<(IEnumerable<Products> Items, int TotalPages, int TotalCount, int Min, int Max, int DbMax)> GetPagedProductsAsync(
            int pageNumber,
            int pageSize,
            int categoryId,
            int min,
            int max,
            string? search,
            string? sortBy,
            bool asc = true);
        Task<Products?> GetProductByIdAsync(int id);

        Task<Products> CreateProductAsync(Products product);

        Task<Products?> UpdateProductAsync(Products product);

        Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);

        Task<bool> ReduceProductStockAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    }
}
