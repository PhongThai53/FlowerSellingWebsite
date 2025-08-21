using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectGreenLens.Repositories.Implementations;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class CartRepository : BaseRepository<Cart>, ICartRepository
    {
        public CartRepository(FlowerSellingDbContext context) : base(context)
        {
        }

        public async Task<Cart?> GetActiveCartByUserIdAsync(int userId)
        {
            return await _context.Cart
                .Where(c => c.UserId == userId && !c.IsCheckedOut && !c.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<Cart?> GetCartWithItemsByIdAsync(int cartId)
        {
            return await _context.Cart
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.ProductPhotos)
                .Include(c => c.User)
                .Where(c => c.Id == cartId && !c.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<Cart?> GetCartWithItemsByUserIdAsync(int userId)
        {
            return await _context.Cart
                .Include(c => c.CartItems.Where(ci => !ci.IsDeleted))
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.ProductPhotos.Where(pp => !pp.IsDeleted))
                .Include(c => c.User)
                .Where(c => c.UserId == userId && !c.IsCheckedOut && !c.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<CartItem?> GetCartItemAsync(int cartId, int productId)
        {
            return await _context.CartItem
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cartId && ci.ProductId == productId && !ci.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<CartItem> AddCartItemAsync(CartItem cartItem)
        {
            await _context.CartItem.AddAsync(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task UpdateCartItemAsync(CartItem cartItem)
        {
            cartItem.UpdatedAt = DateTime.UtcNow;
            cartItem.LineTotal = cartItem.Quantity * cartItem.UnitPrice;
            _context.CartItem.Update(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCartItemAsync(CartItem cartItem)
        {
            cartItem.IsDeleted = true;
            cartItem.UpdatedAt = DateTime.UtcNow;
            _context.CartItem.Update(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<CartItem>> GetPagedCartItemsAsync(int cartId, int page, int pageSize)
        {
            var query = _context.CartItem
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.ProductPhotos.Where(pp => !pp.IsDeleted))
                .Where(ci => ci.CartId == cartId && !ci.IsDeleted)
                .OrderByDescending(ci => ci.CreatedAt);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<CartItem>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = items
            };
        }

        public async Task<int> GetCartItemsCountAsync(int cartId)
        {
            return await _context.CartItem
                .Where(ci => ci.CartId == cartId && !ci.IsDeleted)
                .SumAsync(ci => ci.Quantity);
        }

        public async Task<decimal> GetCartTotalAsync(int cartId)
        {
            return await _context.CartItem
                .Where(ci => ci.CartId == cartId && !ci.IsDeleted)
                .SumAsync(ci => ci.LineTotal);
        }

        public async Task ClearCartAsync(int cartId)
        {
            var cartItems = await _context.CartItem
                .Where(ci => ci.CartId == cartId && !ci.IsDeleted)
                .ToListAsync();

            foreach (var item in cartItems)
            {
                item.IsDeleted = true;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
