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
            // Sử dụng transaction để đảm bảo atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                
                // Commit transaction
                await transaction.CommitAsync();
                
                // Clear change tracker để tránh duplicate
                _context.ChangeTracker.Clear();
                
                return product;
            }
            catch
            {
                // Rollback nếu có lỗi
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id )
        {
            // Get product WITH tracking for update, IGNORING global query filter
            var existing = await _context.Products
                .IgnoreQueryFilters()  // ← IGNORE global query filter
                .Include(p => p.ProductPhotos)
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (existing == null)
            {
                return false;
            }
            
            // Soft delete instead of hard delete
            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;
            
            _context.Products.Update(existing);
            var result = await _context.SaveChangesAsync();
            
            // Log the result
            Console.WriteLine($"DeleteProductAsync: Updated {result} entities for product {id}");
            
            return result > 0;
        }

        public async Task<bool> ActivateProductAsync(int id)
        {
            // Get product WITH tracking for update, IGNORING global query filter
            var existing = await _context.Products
                .IgnoreQueryFilters()  // ← IGNORE global query filter
                .Include(p => p.ProductPhotos)
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (existing == null)
            {
                return false;
            }
            
            // Activate product (soft delete reverse)
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.UpdatedAt = DateTime.UtcNow;
            
            _context.Products.Update(existing);
            var result = await _context.SaveChangesAsync();
            
            // Log the result
            Console.WriteLine($"ActivateProductAsync: Updated {result} entities for product {id}");
            
            return result > 0;
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
                .Include(p => p.ProductCategories)
                .Include(p => p.ProductPhotos)
                .AsNoTracking()
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

            // Allow products with null Price to be displayed
            if (min > 0 || max < int.MaxValue)
            {
                query = query.Where(p => p.Price == null || (p.Price >= min && p.Price <= max));
            }


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
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // Method for admin to get all products including deleted ones
        public async Task<(IEnumerable<Products> Items, int TotalPages, int TotalCount, int Min, int Max, int DbMax)> GetAllProductsIncludingDeletedAsync(
            int pageNumber,
            int pageSize,
            int categoryId,
            int min,
            int max,
            string? search,
            string? sortBy,
            bool asc = true)
        {
            // Use IgnoreQueryFilters() to bypass the global IsDeleted filter
            var query = _context.Products
                                .IgnoreQueryFilters()
                                .Include(p => p.ProductCategories)
                                .Include(p => p.ProductPhotos)
                                .AsNoTracking()
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

            // Allow products with null Price to be displayed
            if (min > 0 || max < int.MaxValue)
            {
                query = query.Where(p => p.Price == null || (p.Price >= min && p.Price <= max));
            }

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

        // Method for admin to get product by ID including deleted ones
        public async Task<Products?> GetProductByIdIncludingDeletedAsync(int id)
        {
            return await _context.Products
                .IgnoreQueryFilters()
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
