using FlowerSellingWebsite.Models.DTOs.Cart;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDTO> GetCartByUserIdAsync(int userId);
        Task<CartDTO> AddToCartAsync(int userId, AddToCartDTO addToCartDto);
        Task<CartItemDTO> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemDTO updateDto);
        Task<bool> RemoveCartItemAsync(int userId, int cartItemId);
        Task<bool> ClearCartAsync(int userId);
        Task<PagedCartResultDTO> GetPagedCartItemsAsync(int userId, int page = 1, int pageSize = 10);
        Task<CartSummaryDTO> GetCartSummaryAsync(int userId);
        Task<int> GetCartItemsCountAsync(int userId);
    }
}





