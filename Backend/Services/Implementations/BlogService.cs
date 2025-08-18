using FlowerSellingWebsite.Models.DTOs.Blog;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Models.Enums;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using ProjectGreenLens.Services.Implementations;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class BlogService : BaseService<Blog>, IBlogService
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IUserRepository _userRepository;

        public BlogService(IBlogRepository blogRepository, IUserRepository userRepository) : base(blogRepository)
        {
            _blogRepository = blogRepository;
            _userRepository = userRepository;
        }

        public async Task<PagedBlogResultDTO> GetBlogsWithFiltersAsync(BlogFilterDTO filters)
        {
            var (blogs, totalCount) = await _blogRepository.GetBlogsWithFiltersAsync(filters);
            
            var blogListDTOs = blogs.Select(MapToBlogListDTO).ToList();
            
            return new PagedBlogResultDTO
            {
                Blogs = blogListDTOs,
                TotalCount = totalCount,
                Page = filters.Page,
                PageSize = filters.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filters.PageSize),
                HasPreviousPage = filters.Page > 1,
                HasNextPage = filters.Page < (int)Math.Ceiling(totalCount / (double)filters.PageSize)
            };
        }

        public async Task<BlogDTO> GetBlogByIdAsync(int id)
        {
            var blog = await _blogRepository.GetBlogWithDetailsAsync(id);
            if (blog == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");
                
            return MapToBlogDTO(blog);
        }

        public async Task<BlogDTO> GetBlogByPublicIdAsync(Guid publicId)
        {
            var blog = await _blogRepository.GetBlogByPublicIdAsync(publicId);
            if (blog == null)
                throw new KeyNotFoundException($"Blog with PublicID {publicId} not found.");
                
            return MapToBlogDTO(blog);
        }

        public async Task<BlogDTO> CreateBlogAsync(CreateBlogDTO createBlogDTO, int userId)
        {
            var blog = new Blog
            {
                Title = createBlogDTO.Title,
                Content = createBlogDTO.Content,
                Tags = createBlogDTO.Tags,
                UserId = userId,
                CategoryId = createBlogDTO.CategoryId,
                Status = BlogStatus.Draft
            };

            var createdBlog = await _blogRepository.createAsync(blog);
            var blogWithDetails = await _blogRepository.GetBlogWithDetailsAsync(createdBlog.Id);
            
            return MapToBlogDTO(blogWithDetails!);
        }

        public async Task<BlogDTO> UpdateBlogAsync(int id, UpdateBlogDTO updateBlogDTO, int currentUserId)
        {
            var existingBlog = await _blogRepository.GetBlogWithDetailsAsync(id);
            if (existingBlog == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            // Check permission: User can only edit their own blogs, Admin can edit all
            if (existingBlog.UserId != currentUserId && !await IsAdminUser(currentUserId))
                throw new UnauthorizedAccessException("You don't have permission to edit this blog.");

            existingBlog.Title = updateBlogDTO.Title;
            existingBlog.Content = updateBlogDTO.Content;
            existingBlog.Tags = updateBlogDTO.Tags;
            existingBlog.CategoryId = updateBlogDTO.CategoryId;
            
            // Reset status to Draft if content changes
            if (existingBlog.Status == BlogStatus.Published || existingBlog.Status == BlogStatus.Rejected)
            {
                existingBlog.Status = BlogStatus.Draft;
                existingBlog.RejectionReason = null;
            }

            await _blogRepository.updateAsync(existingBlog);
            var updatedBlog = await _blogRepository.GetBlogWithDetailsAsync(id);
            
            return MapToBlogDTO(updatedBlog!);
        }

        public async Task<bool> DeleteBlogAsync(int id, int currentUserId)
        {
            var blog = await _blogRepository.getByIdAsync(id);
            if (blog == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            // Check permission: User can only delete their own blogs, Admin can delete all
            if (blog.UserId != currentUserId && !await IsAdminUser(currentUserId))
                throw new UnauthorizedAccessException("You don't have permission to delete this blog.");

            await _blogRepository.deleteAsync(blog);
            return true;
        }

        public async Task<bool> SubmitForApprovalAsync(int id, int currentUserId)
        {
            var blog = await _blogRepository.getByIdAsync(id);
            if (blog == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            if (blog.UserId != currentUserId)
                throw new UnauthorizedAccessException("You can only submit your own blogs for approval.");

            if (blog.Status != BlogStatus.Draft && blog.Status != BlogStatus.Rejected)
                throw new InvalidOperationException("Only draft or rejected blogs can be submitted for approval.");

            blog.Status = BlogStatus.Pending;
            blog.RejectionReason = null;
            await _blogRepository.updateAsync(blog);
            
            return true;
        }

        public async Task<bool> ApproveBlogAsync(int id, int adminId)
        {
            if (!await IsAdminUser(adminId))
                throw new UnauthorizedAccessException("Only admins can approve blogs.");

            var blog = await _blogRepository.getByIdAsync(id);
            if (blog == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            if (blog.Status != BlogStatus.Pending)
                throw new InvalidOperationException("Only pending blogs can be approved.");

            blog.Status = BlogStatus.Published;
            blog.RejectionReason = null;
            await _blogRepository.updateAsync(blog);
            
            return true;
        }

        public async Task<bool> RejectBlogAsync(int id, int adminId, string rejectionReason)
        {
            if (!await IsAdminUser(adminId))
                throw new UnauthorizedAccessException("Only admins can reject blogs.");

            var blog = await _blogRepository.getByIdAsync(id);
            if (blog == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            if (blog.Status != BlogStatus.Pending)
                throw new InvalidOperationException("Only pending blogs can be rejected.");

            blog.Status = BlogStatus.Rejected;
            blog.RejectionReason = rejectionReason;
            await _blogRepository.updateAsync(blog);
            
            return true;
        }

        public async Task<bool> PublishBlogAsync(int id, int currentUserId)
        {
            var blog = await _blogRepository.getByIdAsync(id);
            if (blog == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            // Check permission
            if (blog.UserId != currentUserId && !await IsAdminUser(currentUserId))
                throw new UnauthorizedAccessException("You don't have permission to publish this blog.");

            if (blog.Status != BlogStatus.Draft && blog.Status != BlogStatus.Published)
                throw new InvalidOperationException("Only draft blogs can be published directly by owners.");

            blog.Status = BlogStatus.Published;
            blog.RejectionReason = null;
            await _blogRepository.updateAsync(blog);
            
            return true;
        }

        public async Task<bool> UnpublishBlogAsync(int id, int currentUserId)
        {
            var blog = await _blogRepository.getByIdAsync(id);
            if (blog == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            // Check permission
            if (blog.UserId != currentUserId && !await IsAdminUser(currentUserId))
                throw new UnauthorizedAccessException("You don't have permission to unpublish this blog.");

            blog.Status = BlogStatus.Draft;
            await _blogRepository.updateAsync(blog);
            
            return true;
        }

        public async Task<bool> AddImagesToBlogAsync(int id, List<string> imageUrls, int currentUserId)
        {
            var blog = await _blogRepository.getByIdAsync(id);
            if (blog == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            // Check permission
            if (blog.UserId != currentUserId && !await IsAdminUser(currentUserId))
                throw new UnauthorizedAccessException("You don't have permission to modify this blog.");

            blog.Images.AddRange(imageUrls);
            await _blogRepository.updateAsync(blog);
            
            return true;
        }

        public async Task<bool> RemoveImageFromBlogAsync(int id, string imageUrl, int currentUserId)
        {
            var blog = await _blogRepository.getByIdAsync(id);
            if (blog == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            // Check permission
            if (blog.UserId != currentUserId && !await IsAdminUser(currentUserId))
                throw new UnauthorizedAccessException("You don't have permission to modify this blog.");

            // Convert full URL to relative path if needed
            string relativePath = imageUrl;
            if (imageUrl.StartsWith("http://") || imageUrl.StartsWith("https://"))
            {
                var uri = new Uri(imageUrl);
                relativePath = uri.LocalPath; // This will extract /uploads/blog-images/xxx.png from full URL
            }

            // Remove the image path from blog images
            var removed = blog.Images.Remove(relativePath);
            if (!removed)
            {
                // Try removing the original imageUrl if relative path didn't work
                removed = blog.Images.Remove(imageUrl);
            }

            if (removed)
            {
                await _blogRepository.updateAsync(blog);
            }
            
            return removed;
        }

        public async Task<PagedBlogResultDTO> GetBlogsByStatusAsync(BlogStatus status, int page = 1, int pageSize = 6)
        {
            var blogs = await _blogRepository.GetBlogsByStatusAsync(status, page, pageSize);
            var totalCount = await _blogRepository.GetBlogCountByStatusAsync(status);
            
            var blogListDTOs = blogs.Select(MapToBlogListDTO).ToList();
            
            return new PagedBlogResultDTO
            {
                Blogs = blogListDTOs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = page > 1,
                HasNextPage = page < (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<PagedBlogResultDTO> GetBlogsByUserAsync(int userId, int page = 1, int pageSize = 6)
        {
            var blogs = await _blogRepository.GetBlogsByUserAsync(userId, page, pageSize);
            var totalCount = await _blogRepository.GetBlogCountByUserAsync(userId);
            
            var blogListDTOs = blogs.Select(MapToBlogListDTO).ToList();
            
            return new PagedBlogResultDTO
            {
                Blogs = blogListDTOs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = page > 1,
                HasNextPage = page < (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<PagedBlogResultDTO> GetBlogsByCategoryAsync(int categoryId, int page = 1, int pageSize = 6)
        {
            var blogs = await _blogRepository.GetBlogsByCategoryAsync(categoryId, page, pageSize);
            var totalCount = await _blogRepository.GetBlogCountByCategoryAsync(categoryId);
            
            var blogListDTOs = blogs.Select(MapToBlogListDTO).ToList();
            
            return new PagedBlogResultDTO
            {
                Blogs = blogListDTOs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = page > 1,
                HasNextPage = page < (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<PagedBlogResultDTO> GetBlogsForCurrentUserAsync(BlogFilterDTO filters, int currentUserId)
        {
            // For regular users: get published blogs + their own blogs (any status)
            var (blogs, totalCount) = await _blogRepository.GetBlogsForUserWithPermissionAsync(filters, currentUserId);
            
            var blogListDTOs = blogs.Select(MapToBlogListDTO).ToList();
            
            return new PagedBlogResultDTO
            {
                Blogs = blogListDTOs,
                TotalCount = totalCount,
                Page = filters.Page,
                PageSize = filters.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filters.PageSize),
                HasPreviousPage = filters.Page > 1,
                HasNextPage = filters.Page < (int)Math.Ceiling(totalCount / (double)filters.PageSize)
            };
        }

        private BlogDTO MapToBlogDTO(Blog blog)
        {
            return new BlogDTO
            {
                Id = blog.Id,
                PublicId = blog.PublicId,
                Title = blog.Title,
                Content = blog.Content,
                Tags = blog.Tags,
                Images = blog.Images,
                Status = blog.Status,
                RejectionReason = blog.RejectionReason,
                UserId = blog.UserId,
                UserName = blog.User?.UserName ?? "",
                UserFullName = blog.User?.FullName ?? "",
                CategoryId = blog.CategoryId,
                CategoryName = blog.Category?.CategoryName ?? "",
                CommentCount = blog.Comments?.Count(c => !c.IsHide) ?? 0,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt
            };
        }

        private BlogListDTO MapToBlogListDTO(Blog blog)
        {
            return new BlogListDTO
            {
                Id = blog.Id,
                PublicId = blog.PublicId,
                Title = blog.Title,
                Tags = blog.Tags,
                Images = blog.Images,
                Status = blog.Status,
                UserId = blog.UserId,
                UserName = blog.User?.UserName ?? "",
                UserFullName = blog.User?.FullName ?? "",
                CategoryId = blog.CategoryId,
                CategoryName = blog.Category?.CategoryName ?? "",
                CommentCount = blog.Comments?.Count(c => !c.IsHide) ?? 0,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt
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
