using FlowerSellingWebsite.Models.Entities;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface IProductRepository
    {
        public Task<(IEnumerable<Products> Items, int TotalPages, int TotalCount)> GetPagedProductsAsync(
            int pageNumber,
            int pageSize,
            string? search,
            string? sortBy,
            bool asc = true,
            CancellationToken cancellationToken = default);
        Task<Products?> GetProductByIdAsync(int id);
    }
}
