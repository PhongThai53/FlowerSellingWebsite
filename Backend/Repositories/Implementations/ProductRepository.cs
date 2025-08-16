using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly FlowerSellingDbContext _context;
        public ProductRepository(FlowerSellingDbContext context)
        {
            _context = context;
        }
        public async Task<(IEnumerable<Products> Items, int TotalPages, int TotalCount)> GetPagedProductsAsync(
           int pageNumber,
           int pageSize,
           string? search,
           string? sortBy,
           bool asc = true,
           CancellationToken cancellationToken = default)
        {
            // Query
            var query = _context.Products
                                .Where(p => !p.IsDeleted)
                                .Include(p => p.ProductCategories)
                                .Include(p => p.ProductPhotos)
                                .AsQueryable();
            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    (p.Name.Contains(search)) ||
                    (p.Description != null && p.Description.Contains(search)));
            }

            // Count
            var totalCount = await query.CountAsync();

            // Sort
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    "name" => asc ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                    "price" => asc ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                    "created" => asc ? query.OrderBy(p => p.ProductCategories.Name) : query.OrderByDescending(p => p.ProductCategories.Name),
                    _ => query.OrderBy(p => p.Id)
                };
            }
            else
            {
                query = query.OrderBy(p => p.Id);
            }

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var items = await query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, totalPages, totalCount);
        }

        public async Task<Products?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductPhotos)
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
