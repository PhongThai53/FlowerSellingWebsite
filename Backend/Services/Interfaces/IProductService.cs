using FlowerSellingWebsite.Models.DTOs.Product;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IProductService
    {
        public Task<(IEnumerable<ProductDTO> Items, int TotalPages, int TotalCount, int Min, int Max, int DbMax)> GetPagedProductsAsync(
            int pageNumber,
            int pageSize,
            int categoryId,
            int min,
            int max,
            string? search,
            string? sortBy,
            bool asc = true);
        Task<ProductDTO?> GetProductByIdAsync(int id);

        Task<CreateProductDTO?> CreateProductAsync(CreateProductDTO dto);

        Task<UpdateProductDTO?> UpdateProductAsync(int id, UpdateProductDTO dto);

        Task<bool> DeleteProductAsync(int id);

        //Task<bool> ReduceStockForOrderAsync(List<(int ProductId, int Quantity)> orderItems, CancellationToken cancellationToken = default);
    }
}
