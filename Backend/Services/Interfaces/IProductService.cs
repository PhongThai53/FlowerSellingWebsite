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
            
        // Admin method to get all products including deleted ones
        public Task<(IEnumerable<ProductDTO> Items, int TotalPages, int TotalCount, int Min, int Max, int DbMax)> GetAllProductsIncludingDeletedAsync(
            int pageNumber,
            int pageSize,
            int categoryId,
            int min,
            int max,
            string? search,
            string? sortBy,
            bool asc = true);
            
        Task<ProductDTO?> GetProductByIdAsync(int id);
        Task<CreateProductDTO?> CreateProductAsync(CreateProductDTO createProductDTO);
        Task<UpdateProductDTO?> UpdateProductAsync(int id, UpdateProductDTO updateProductDTO);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> ActivateProductAsync(int id);
        Task<ProductAvailabilityDTO> CheckProductAvailabilityAsync(int productId, int quantity);
    }
}
