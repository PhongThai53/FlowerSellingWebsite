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

        public async Task<Products> CreateProductAsync(Products product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var existing = await GetProductByIdAsync(id);
            if (existing == null)
            {
                return false;
            }
            _context.Products.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<Products> Items, int TotalPages, int TotalCount, int Min, int Max, int DbMax)> GetPagedProductsAsync(
            int pageNumber,
            int pageSize,
            int categoryId,
            int min,
            int max,
            string? search,
            string? sortBy,
            bool asc = true)
        {
            var query = _context.Products
                                .Where(p => !p.IsDeleted)
                                .Include(p => p.ProductCategories)
                                .Include(p => p.ProductPhotos)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    (p.Description != null && p.Description.Contains(search)));
            }

            if (categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            int DbMax = Convert.ToInt32(await query.AnyAsync() ? await query.MaxAsync(p => p.Price) ?? 0 : 0);

            query = query.Where(p => p.Price >= min && p.Price <= max);
            query = query.Where(p => p.Price >= min && p.Price <= max);


            var totalCount = await query.CountAsync();

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

            // Paging
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return (items, totalPages, totalCount, min, max, DbMax);
        }

        public async Task<Products?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductPhotos)
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Products?> UpdateProductAsync(Products product)
        {
            var existing = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(product);

            var photos = await _context.ProductPhotos
                .Where(pp => pp.ProductId == product.Id && pp.IsPrimary)
                .ToListAsync();

            if (photos.Any())
            {
                foreach (var photo in photos)
                {
                    photo.IsPrimary = false;
                }
            }

            await _context.SaveChangesAsync();
            return existing;
        }
    }
}
