using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Models.DTOs;
using ProjectGreenLens.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface ICartRepository : IBaseRepository<Cart>
    {
        Task<Cart?> GetActiveCartByUserIdAsync(int userId);
        Task<Cart?> GetCartWithItemsByIdAsync(int cartId);
        Task<Cart?> GetCartWithItemsByUserIdAsync(int userId);
        Task<CartItem?> GetCartItemAsync(int cartId, int productId);
        Task<CartItem> AddCartItemAsync(CartItem cartItem);
        Task UpdateCartItemAsync(CartItem cartItem);
        Task DeleteCartItemAsync(CartItem cartItem);
        Task<PagedResult<CartItem>> GetPagedCartItemsAsync(int cartId, int page, int pageSize);
        Task<int> GetCartItemsCountAsync(int cartId);
        Task<decimal> GetCartTotalAsync(int cartId);
        Task ClearCartAsync(int cartId);
    }
}





