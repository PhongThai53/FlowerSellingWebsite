using FlowerSellingWebsite.Models.DTOs.Comment;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface ICommentService
    {
        // Comment CRUD operations
        Task<CommentDTO> GetCommentByIdAsync(int id);
        Task<CommentDTO> CreateCommentAsync(CreateCommentDTO createCommentDTO, int userId);
        Task<CommentDTO> UpdateCommentAsync(int id, UpdateCommentDTO updateCommentDTO, int currentUserId);
        Task<bool> DeleteCommentAsync(int id, int currentUserId);

        // Comment visibility management
        Task<bool> HideCommentAsync(int id, int currentUserId);
        Task<bool> ShowCommentAsync(int id, int currentUserId);

        // Get comments by blog
        Task<List<CommentDTO>> GetCommentsByBlogIdAsync(int blogId);
        Task<List<CommentDTO>> GetCommentsByParentIdAsync(int parentId);

        // Get comments by user
        Task<List<CommentDTO>> GetCommentsByUserIdAsync(int userId);
    }
}
