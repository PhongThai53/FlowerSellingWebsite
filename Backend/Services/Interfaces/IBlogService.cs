using FlowerSellingWebsite.Models.DTOs.Blog;
using FlowerSellingWebsite.Models.Enums;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IBlogService
    {
        // Blog CRUD operations
        Task<PagedBlogResultDTO> GetBlogsWithFiltersAsync(BlogFilterDTO filters);
        Task<BlogDTO> GetBlogByIdAsync(int id);
        Task<BlogDTO> GetBlogByPublicIdAsync(Guid publicId);
        Task<BlogDTO> CreateBlogAsync(CreateBlogDTO createBlogDTO, int userId);
        Task<BlogDTO> UpdateBlogAsync(int id, UpdateBlogDTO updateBlogDTO, int currentUserId);
        Task<bool> DeleteBlogAsync(int id, int currentUserId);

        // Blog approval workflow
        Task<bool> SubmitForApprovalAsync(int id, int currentUserId);
        Task<bool> ApproveBlogAsync(int id, int adminId);
        Task<bool> RejectBlogAsync(int id, int adminId, string rejectionReason);

        // Blog status management
        Task<bool> PublishBlogAsync(int id, int currentUserId);
        Task<bool> UnpublishBlogAsync(int id, int currentUserId);

        // Image management
        Task<bool> AddImagesToBlogAsync(int id, List<string> imageUrls, int currentUserId);
        Task<bool> RemoveImageFromBlogAsync(int id, string imageUrl, int currentUserId);

        // Get blogs by status
        Task<PagedBlogResultDTO> GetBlogsByStatusAsync(BlogStatus status, int page = 1, int pageSize = 6);
        Task<PagedBlogResultDTO> GetBlogsByUserAsync(int userId, int page = 1, int pageSize = 6);
        Task<PagedBlogResultDTO> GetBlogsByCategoryAsync(int categoryId, int page = 1, int pageSize = 6);
    }
}
