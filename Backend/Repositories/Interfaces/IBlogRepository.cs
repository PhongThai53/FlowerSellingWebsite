using FlowerSellingWebsite.Models.DTOs.Blog;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Models.Enums;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface IBlogRepository : IBaseRepository<Blog>
    {
        Task<(List<Blog> blogs, int totalCount)> GetBlogsWithFiltersAsync(BlogFilterDTO filters);
        Task<Blog?> GetBlogWithDetailsAsync(int id);
        Task<Blog?> GetBlogByPublicIdAsync(Guid publicId);
        Task<List<Blog>> GetBlogsByStatusAsync(BlogStatus status, int page, int pageSize);
        Task<List<Blog>> GetBlogsByUserAsync(int userId, int page, int pageSize);
        Task<List<Blog>> GetBlogsByCategoryAsync(int categoryId, int page, int pageSize);
        Task<int> GetBlogCountByUserAsync(int userId);
        Task<int> GetBlogCountByCategoryAsync(int categoryId);
        Task<int> GetBlogCountByStatusAsync(BlogStatus status);
        
        // Get blogs with user permission (published + own blogs)
        Task<(List<Blog> blogs, int totalCount)> GetBlogsForUserWithPermissionAsync(BlogFilterDTO filters, int currentUserId);
    }
}
