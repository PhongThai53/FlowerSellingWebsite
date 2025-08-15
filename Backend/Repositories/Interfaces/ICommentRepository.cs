using FlowerSellingWebsite.Models.Entities;
using ProjectGreenLens.Repositories.Interfaces;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface ICommentRepository : IBaseRepository<Comment>
    {
        Task<Comment?> GetCommentWithDetailsAsync(int id);
        Task<List<Comment>> GetCommentsByBlogIdAsync(int blogId);
        Task<List<Comment>> GetCommentsByParentIdAsync(int parentId);
        Task<List<Comment>> GetCommentsByUserIdAsync(int userId);
        Task<int> GetCommentCountByBlogIdAsync(int blogId);
        Task<int> GetCommentCountByUserIdAsync(int userId);
    }
}
