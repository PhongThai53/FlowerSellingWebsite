using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            // LineTotal is computed by the database, don't set it manually
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

        public async Task ClearCartByUserIdAsync(int userId)
        {
            try
            {
                // Get the active cart for the user
                var activeCart = await GetActiveCartByUserIdAsync(userId);
                if (activeCart == null)
                {
                    Console.WriteLine($"No active cart found for user {userId}");
                    return;
                }

                // Clear all cart items
                var cartItems = await _context.CartItem
                    .Where(ci => ci.CartId == activeCart.Id && !ci.IsDeleted)
                    .ToListAsync();

                foreach (var item in cartItems)
                {
                    item.IsDeleted = true;
                    item.UpdatedAt = DateTime.UtcNow;
                }

                // Mark the cart as checked out
                activeCart.IsCheckedOut = true;
                activeCart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                Console.WriteLine($"Successfully cleared cart for user {userId}. {cartItems.Count} items removed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing cart for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<Cart> CreateNewCartForUserAsync(int userId)
        {
            try
            {
                var newCart = new Cart
                {
                    UserId = userId,
                    IsCheckedOut = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Cart.Add(newCart);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Created new cart {newCart.Id} for user {userId}");
                return newCart;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating new cart for user {userId}: {ex.Message}");
                throw;
            }
        }
    }
}
