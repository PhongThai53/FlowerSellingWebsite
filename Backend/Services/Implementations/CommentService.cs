using FlowerSellingWebsite.Models.DTOs.Comment;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IBlogRepository _blogRepository;
        private readonly IUserRepository _userRepository;

        public CommentService(
            ICommentRepository commentRepository, 
            IBlogRepository blogRepository,
            IUserRepository userRepository) : base(commentRepository)
        {
            _commentRepository = commentRepository;
            _blogRepository = blogRepository;
            _userRepository = userRepository;
        }

        public async Task<CommentDTO> GetCommentByIdAsync(int id)
        {
            var comment = await _commentRepository.GetCommentWithDetailsAsync(id);
            if (comment == null)
                throw new KeyNotFoundException($"Comment with ID {id} not found.");

            return MapToCommentDTO(comment);
        }

        public async Task<CommentDTO> CreateCommentAsync(CreateCommentDTO createCommentDTO, int userId)
        {
            // Check if blog exists
            var blog = await _blogRepository.getByIdAsync(createCommentDTO.BlogId);
            if (blog == null)
                throw new KeyNotFoundException($"Blog with ID {createCommentDTO.BlogId} not found.");

            // Check if parent comment exists (if provided)
            if (createCommentDTO.ParentId.HasValue)
            {
                var parentComment = await _commentRepository.getByIdAsync(createCommentDTO.ParentId.Value);
                if (parentComment == null)
                    throw new KeyNotFoundException($"Parent comment with ID {createCommentDTO.ParentId.Value} not found.");

                // Ensure parent comment belongs to the same blog
                if (parentComment.BlogId != createCommentDTO.BlogId)
                    throw new InvalidOperationException("Parent comment must belong to the same blog.");
            }

            var comment = new Comment
            {
                Content = createCommentDTO.Content,
                BlogId = createCommentDTO.BlogId,
                ParentId = createCommentDTO.ParentId,
                UserId = userId,
                IsHide = false
            };

            var createdComment = await _commentRepository.createAsync(comment);
            var commentWithDetails = await _commentRepository.GetCommentWithDetailsAsync(createdComment.Id);

            return MapToCommentDTO(commentWithDetails!);
        }

        public async Task<CommentDTO> UpdateCommentAsync(int id, UpdateCommentDTO updateCommentDTO, int currentUserId)
        {
            var existingComment = await _commentRepository.GetCommentWithDetailsAsync(id);
            if (existingComment == null)
                throw new KeyNotFoundException($"Comment with ID {id} not found.");

            // Check permission: User can only edit their own comments, Admin can edit all
            if (existingComment.UserId != currentUserId && !await IsAdminUser(currentUserId))
                throw new UnauthorizedAccessException("You don't have permission to edit this comment.");

            existingComment.Content = updateCommentDTO.Content;
            await _commentRepository.updateAsync(existingComment);

            var updatedComment = await _commentRepository.GetCommentWithDetailsAsync(id);
            return MapToCommentDTO(updatedComment!);
        }

        public async Task<bool> DeleteCommentAsync(int id, int currentUserId)
        {
            var comment = await _commentRepository.getByIdAsync(id);
            if (comment == null)
                throw new KeyNotFoundException($"Comment with ID {id} not found.");

            // Check permission: User can only delete their own comments, Admin can delete all
            if (comment.UserId != currentUserId && !await IsAdminUser(currentUserId))
                throw new UnauthorizedAccessException("You don't have permission to delete this comment.");

            await _commentRepository.deleteAsync(comment);
            return true;
        }

        public async Task<bool> HideCommentAsync(int id, int currentUserId)
        {
            var comment = await _commentRepository.GetCommentWithDetailsAsync(id);
            if (comment == null)
                throw new KeyNotFoundException($"Comment with ID {id} not found.");

            // Check permission: Blog owner or Admin can hide comments
            if (comment.Blog.UserId != currentUserId && !await IsAdminUser(currentUserId))
                throw new UnauthorizedAccessException("You don't have permission to hide this comment.");

            comment.IsHide = true;
            await _commentRepository.updateAsync(comment);
            return true;
        }

        public async Task<bool> ShowCommentAsync(int id, int currentUserId)
        {
            var comment = await _commentRepository.GetCommentWithDetailsAsync(id);
            if (comment == null)
                throw new KeyNotFoundException($"Comment with ID {id} not found.");

            // Check permission: Blog owner or Admin can show comments
            if (comment.Blog.UserId != currentUserId && !await IsAdminUser(currentUserId))
                throw new UnauthorizedAccessException("You don't have permission to show this comment.");

            comment.IsHide = false;
            await _commentRepository.updateAsync(comment);
            return true;
        }

        public async Task<List<CommentDTO>> GetCommentsByBlogIdAsync(int blogId)
        {
            var comments = await _commentRepository.GetCommentsByBlogIdAsync(blogId);
            return comments.Select(MapToCommentDTO).ToList();
        }
        
        public async Task<List<CommentDTO>> GetAllCommentsByBlogIdAsync(int blogId, int currentUserId)
        {
            // Check if user is admin or blog owner
            var blog = await _blogRepository.getByIdAsync(blogId);
            if (blog == null)
                throw new KeyNotFoundException($"Blog with ID {blogId} not found.");
                
            bool isAdminOrOwner = blog.UserId == currentUserId || await IsAdminUser(currentUserId);
            
            // If user is admin or blog owner, get all comments including hidden ones
            if (isAdminOrOwner)
            {
                var allComments = await _commentRepository.GetAllCommentsByBlogIdAsync(blogId);
                return allComments.Select(MapToCommentDTO).ToList();
            }
            else
            {
                // Otherwise, get only visible comments
                var visibleComments = await _commentRepository.GetCommentsByBlogIdAsync(blogId);
                return visibleComments.Select(MapToCommentDTO).ToList();
            }
        }

        public async Task<List<CommentDTO>> GetCommentsByParentIdAsync(int parentId)
        {
            var comments = await _commentRepository.GetCommentsByParentIdAsync(parentId);
            return comments.Select(MapToCommentDTO).ToList();
        }

        public async Task<List<CommentDTO>> GetCommentsByUserIdAsync(int userId)
        {
            var comments = await _commentRepository.GetCommentsByUserIdAsync(userId);
            return comments.Select(MapToCommentDTO).ToList();
        }

        private CommentDTO MapToCommentDTO(Comment comment)
        {
            return new CommentDTO
            {
                Id = comment.Id,
                PublicId = comment.PublicId,
                Content = comment.Content,
                IsHide = comment.IsHide,
                BlogId = comment.BlogId,
                ParentId = comment.ParentId,
                UserId = comment.UserId,
                UserName = comment.User?.UserName ?? "",
                UserFullName = comment.User?.FullName ?? "",
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                Children = comment.Children?.Select(MapToCommentDTO).ToList() ?? new List<CommentDTO>()
            };
        }

        private async Task<bool> IsAdminUser(int userId)
        {
            // Get user's role from the database
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;
                
            // Check if user has Admin role
            return user.Role?.RoleName == "Admin";
        }
    }
}
