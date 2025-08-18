using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.DTOs.Blog;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Models.Enums;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectGreenLens.Repositories.Implementations;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class BlogRepository : BaseRepository<Blog>, IBlogRepository
    {
        public BlogRepository(FlowerSellingDbContext context) : base(context)
        {
        }

        public async Task<(List<Blog> blogs, int totalCount)> GetBlogsWithFiltersAsync(BlogFilterDTO filters)
        {
            var query = _context.Blogs
                .Include(b => b.User)
                .Include(b => b.Category)
                .Include(b => b.Comments.Where(c => !c.IsHide))
                .Where(b => !b.IsDeleted);

            // Apply filters
            if (!string.IsNullOrEmpty(filters.Search))
            {
                query = query.Where(b => b.Title.Contains(filters.Search) ||
                                        b.Content.Contains(filters.Search) ||
                                        b.Tags.Contains(filters.Search));
            }

            if (filters.CategoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == filters.CategoryId.Value);
            }

            if (filters.UserId.HasValue)
            {
                query = query.Where(b => b.UserId == filters.UserId.Value);
            }

            if (filters.Status.HasValue)
            {
                query = query.Where(b => b.Status == filters.Status.Value);
            }

            if (!string.IsNullOrEmpty(filters.Tags))
            {
                query = query.Where(b => b.Tags.Contains(filters.Tags));
            }

            if (filters.CreatedFrom.HasValue)
            {
                query = query.Where(b => b.CreatedAt >= filters.CreatedFrom.Value);
            }

            if (filters.CreatedTo.HasValue)
            {
                query = query.Where(b => b.CreatedAt <= filters.CreatedTo.Value);
            }

            // Apply sorting
            query = filters.SortBy.ToLower() switch
            {
                "title" => filters.SortOrder.ToLower() == "asc" ? query.OrderBy(b => b.Title) : query.OrderByDescending(b => b.Title),
                "updatedat" => filters.SortOrder.ToLower() == "asc" ? query.OrderBy(b => b.UpdatedAt) : query.OrderByDescending(b => b.UpdatedAt),
                _ => filters.SortOrder.ToLower() == "asc" ? query.OrderBy(b => b.CreatedAt) : query.OrderByDescending(b => b.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var blogs = await query
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            return (blogs, totalCount);
        }

        public async Task<Blog?> GetBlogWithDetailsAsync(int id)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.Category)
                .Include(b => b.Comments.Where(c => !c.IsHide))
                    .ThenInclude(c => c.User)
                .Include(b => b.Comments.Where(c => !c.IsHide))
                    .ThenInclude(c => c.Children.Where(ch => !ch.IsHide))
                        .ThenInclude(ch => ch.User)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        }

        public async Task<Blog?> GetBlogByPublicIdAsync(Guid publicId)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.Category)
                .Include(b => b.Comments.Where(c => !c.IsHide))
                    .ThenInclude(c => c.User)
                .Include(b => b.Comments.Where(c => !c.IsHide))
                    .ThenInclude(c => c.Children.Where(ch => !ch.IsHide))
                        .ThenInclude(ch => ch.User)
                .FirstOrDefaultAsync(b => b.PublicId == publicId && !b.IsDeleted);
        }

        public async Task<List<Blog>> GetBlogsByStatusAsync(BlogStatus status, int page, int pageSize)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.Category)
                .Include(b => b.Comments.Where(c => !c.IsHide))
                .Where(b => b.Status == status && !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Blog>> GetBlogsByUserAsync(int userId, int page, int pageSize)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.Category)
                .Include(b => b.Comments.Where(c => !c.IsHide))
                .Where(b => b.UserId == userId && !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Blog>> GetBlogsByCategoryAsync(int categoryId, int page, int pageSize)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.Category)
                .Include(b => b.Comments.Where(c => !c.IsHide))
                .Where(b => b.CategoryId == categoryId && !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetBlogCountByUserAsync(int userId)
        {
            return await _context.Blogs
                .CountAsync(b => b.UserId == userId && !b.IsDeleted);
        }

        public async Task<int> GetBlogCountByCategoryAsync(int categoryId)
        {
            return await _context.Blogs
                .CountAsync(b => b.CategoryId == categoryId && !b.IsDeleted);
        }

        public async Task<int> GetBlogCountByStatusAsync(BlogStatus status)
        {
            return await _context.Blogs
                .CountAsync(b => b.Status == status && !b.IsDeleted);
        }

        public async Task<(List<Blog> blogs, int totalCount)> GetBlogsForUserWithPermissionAsync(BlogFilterDTO filters, int currentUserId)
        {
            var query = _context.Blogs
                .Include(b => b.User)
                .Include(b => b.Category)
                .Include(b => b.Comments.Where(c => !c.IsHide))
                .Where(b => !b.IsDeleted);

            // User can see: Published blogs OR their own blogs (any status)
            query = query.Where(b => b.Status == BlogStatus.Published || b.UserId == currentUserId);

            // Apply other filters (search, category, etc.) but NOT userId or status filters
            // since we handle those specifically above
            if (!string.IsNullOrEmpty(filters.Search))
            {
                query = query.Where(b => b.Title.Contains(filters.Search) ||
                                        b.Content.Contains(filters.Search) ||
                                        b.Tags.Contains(filters.Search));
            }

            if (filters.CategoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == filters.CategoryId.Value);
            }

            if (!string.IsNullOrEmpty(filters.Tags))
            {
                query = query.Where(b => b.Tags.Contains(filters.Tags));
            }

            if (filters.CreatedFrom.HasValue)
            {
                query = query.Where(b => b.CreatedAt >= filters.CreatedFrom.Value);
            }

            if (filters.CreatedTo.HasValue)
            {
                query = query.Where(b => b.CreatedAt <= filters.CreatedTo.Value);
            }

            // Apply sorting
            query = filters.SortBy.ToLower() switch
            {
                "title" => filters.SortOrder.ToLower() == "asc" ? query.OrderBy(b => b.Title) : query.OrderByDescending(b => b.Title),
                "updatedat" => filters.SortOrder.ToLower() == "asc" ? query.OrderBy(b => b.UpdatedAt) : query.OrderByDescending(b => b.UpdatedAt),
                _ => filters.SortOrder.ToLower() == "asc" ? query.OrderBy(b => b.CreatedAt) : query.OrderByDescending(b => b.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var blogs = await query
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            return (blogs, totalCount);
        }
    }
}
