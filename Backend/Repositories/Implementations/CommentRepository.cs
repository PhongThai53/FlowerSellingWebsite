using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(FlowerSellingDbContext context) : base(context)
        {
        }

        public async Task<Comment?> GetCommentWithDetailsAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Blog)
                .Include(c => c.Parent)
                .Include(c => c.Children.Where(ch => !ch.IsDeleted && !ch.IsHide))
                    .ThenInclude(ch => ch.User)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<List<Comment>> GetCommentsByBlogIdAsync(int blogId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Children.Where(ch => !ch.IsDeleted && !ch.IsHide))
                    .ThenInclude(ch => ch.User)
                .Where(c => c.BlogId == blogId && !c.IsDeleted && !c.IsHide && c.ParentId == null)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<List<Comment>> GetAllCommentsByBlogIdAsync(int blogId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Children.Where(ch => !ch.IsDeleted)) // Include all non-deleted children
                    .ThenInclude(ch => ch.User)
                .Where(c => c.BlogId == blogId && !c.IsDeleted && c.ParentId == null) // Don't filter by IsHide
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetCommentsByParentIdAsync(int parentId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Children.Where(ch => !ch.IsDeleted && !ch.IsHide))
                    .ThenInclude(ch => ch.User)
                .Where(c => c.ParentId == parentId && !c.IsDeleted && !c.IsHide)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetCommentsByUserIdAsync(int userId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Blog)
                .Include(c => c.Parent)
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetCommentCountByBlogIdAsync(int blogId)
        {
            return await _context.Comments
                .CountAsync(c => c.BlogId == blogId && !c.IsDeleted && !c.IsHide);
        }

        public async Task<int> GetCommentCountByUserIdAsync(int userId)
        {
            return await _context.Comments
                .CountAsync(c => c.UserId == userId && !c.IsDeleted);
        }
    }
}
