using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Blog;
using FlowerSellingWebsite.Models.Enums;
using FlowerSellingWebsite.Services.Interfaces;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowerSellingWebsite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<BlogController> _logger;

        public BlogController(IBlogService blogService, IFileUploadService fileUploadService, IUserRepository userRepository, ILogger<BlogController> logger)
        {
            _blogService = blogService;
            _fileUploadService = fileUploadService;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint to verify the controller is working
        /// </summary>
        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            return Ok("Blog Controller is working!");
        }

        /// <summary>
        /// Get blogs with filtering, searching, sorting and pagination (Admin view - all blogs)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PagedBlogResultDTO>>> GetBlogs([FromQuery] BlogFilterDTO filters)
        {
            try
            {
                var result = await _blogService.GetBlogsWithFiltersAsync(filters);
                return Ok(ApiResponse<PagedBlogResultDTO>.Ok(result, "Blogs retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blogs with filters");
                return StatusCode(500, ApiResponse<PagedBlogResultDTO>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Get blogs for current user (published blogs + own blogs with any status)
        /// </summary>
        [HttpGet("user-view")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedBlogResultDTO>>> GetBlogsForCurrentUser([FromQuery] BlogFilterDTO filters)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var result = await _blogService.GetBlogsForCurrentUserAsync(filters, userId);
                return Ok(ApiResponse<PagedBlogResultDTO>.Ok(result, "Blogs retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blogs for current user");
                return StatusCode(500, ApiResponse<PagedBlogResultDTO>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Get blog by ID with full details including comments
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<BlogDTO>>> GetBlogById(int id)
        {
            try
            {
                var blog = await _blogService.GetBlogByIdAsync(id);
                return Ok(ApiResponse<BlogDTO>.Ok(blog, "Blog retrieved successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BlogDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting blog with ID {id}");
                return StatusCode(500, ApiResponse<BlogDTO>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Get blog by Public ID
        /// </summary>
        [HttpGet("public/{publicId}")]
        public async Task<ActionResult<ApiResponse<BlogDTO>>> GetBlogByPublicId(Guid publicId)
        {
            try
            {
                var blog = await _blogService.GetBlogByPublicIdAsync(publicId);
                return Ok(ApiResponse<BlogDTO>.Ok(blog, "Blog retrieved successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BlogDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting blog with PublicID {publicId}");
                return StatusCode(500, ApiResponse<BlogDTO>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Create a new blog
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<BlogDTO>>> CreateBlog([FromBody] CreateBlogDTO createBlogDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<BlogDTO>.Fail("Invalid input data"));
                }

                var userId = await GetCurrentUserIdAsync();
                var createdBlog = await _blogService.CreateBlogAsync(createBlogDTO, userId);
                
                return CreatedAtAction(nameof(GetBlogById), new { id = createdBlog.Id }, 
                    ApiResponse<BlogDTO>.Ok(createdBlog, "Blog created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog");
                return StatusCode(500, ApiResponse<BlogDTO>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Update an existing blog
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<BlogDTO>>> UpdateBlog(int id, [FromBody] UpdateBlogDTO updateBlogDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<BlogDTO>.Fail("Invalid input data"));
                }

                var userId = await GetCurrentUserIdAsync();
                var updatedBlog = await _blogService.UpdateBlogAsync(id, updateBlogDTO, userId);
                
                return Ok(ApiResponse<BlogDTO>.Ok(updatedBlog, "Blog updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BlogDTO>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating blog with ID {id}");
                return StatusCode(500, ApiResponse<BlogDTO>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Delete a blog
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBlog(int id)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var result = await _blogService.DeleteBlogAsync(id, userId);
                
                return Ok(ApiResponse<bool>.Ok(result, "Blog deleted successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting blog with ID {id}");
                return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Submit blog for approval (workflow)
        /// </summary>
        [HttpPost("{id}/submit-for-approval")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> SubmitForApproval(int id)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var result = await _blogService.SubmitForApprovalAsync(id, userId);
                
                return Ok(ApiResponse<bool>.Ok(result, "Blog submitted for approval successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error submitting blog {id} for approval");
                return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Approve a blog (Admin only)
        /// </summary>
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> ApproveBlog(int id)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var result = await _blogService.ApproveBlogAsync(id, userId);
                
                return Ok(ApiResponse<bool>.Ok(result, "Blog approved successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving blog {id}");
                return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Reject a blog (Admin only)
        /// </summary>
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> RejectBlog(int id, [FromBody] RejectBlogRequestDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RejectionReason))
                {
                    return BadRequest(ApiResponse<bool>.Fail("Rejection reason is required"));
                }

                var userId = await GetCurrentUserIdAsync();
                var result = await _blogService.RejectBlogAsync(id, userId, request.RejectionReason);
                
                return Ok(ApiResponse<bool>.Ok(result, "Blog rejected successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting blog {id}");
                return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Publish blog (for blog owners)
        /// </summary>
        [HttpPost("{id}/publish")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> PublishBlog(int id)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var result = await _blogService.PublishBlogAsync(id, userId);
                
                return Ok(ApiResponse<bool>.Ok(result, "Blog published successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error publishing blog {id}");
                return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Unpublish blog
        /// </summary>
        [HttpPost("{id}/unpublish")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> UnpublishBlog(int id)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var result = await _blogService.UnpublishBlogAsync(id, userId);
                
                return Ok(ApiResponse<bool>.Ok(result, "Blog unpublished successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error unpublishing blog {id}");
                return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Upload images for blog
        /// </summary>
        [HttpPost("{id}/upload-images")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<string>>>> UploadImages(int id, [FromForm] IFormFileCollection files)
        {
            try
            {
                _logger.LogInformation("Upload images called for blog {BlogId} with {FileCount} files", id, files?.Count ?? 0);
                
                if (files == null || files.Count == 0)
                {
                    _logger.LogWarning("No files uploaded for blog {BlogId}", id);
                    return BadRequest(ApiResponse<List<string>>.Fail("No files uploaded"));
                }

                var userId = await GetCurrentUserIdAsync();
                _logger.LogInformation("User {UserId} uploading {FileCount} files to blog {BlogId}", userId, files.Count, id);
                
                var imageUrls = await _fileUploadService.UploadMultipleImagesAsync(files);
                _logger.LogInformation("FileUploadService returned {ImageCount} URLs: {ImageUrls}", imageUrls.Count, string.Join(", ", imageUrls));
                
                if (imageUrls.Any())
                {
                    await _blogService.AddImagesToBlogAsync(id, imageUrls, userId);
                    _logger.LogInformation("Successfully added {ImageCount} images to blog {BlogId}", imageUrls.Count, id);
                }
                else
                {
                    _logger.LogWarning("No images were uploaded successfully for blog {BlogId}", id);
                }
                
                return Ok(ApiResponse<List<string>>.Ok(imageUrls, "Images uploaded successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<List<string>>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading images for blog {id}");
                return StatusCode(500, ApiResponse<List<string>>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Remove image from blog
        /// </summary>
        [HttpDelete("{id}/images")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveImage(int id, [FromBody] RemoveImageRequestDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ImageUrl))
                {
                    return BadRequest(ApiResponse<bool>.Fail("Image URL is required"));
                }

                var userId = await GetCurrentUserIdAsync();
                var result = await _blogService.RemoveImageFromBlogAsync(id, request.ImageUrl, userId);
                
                // Also delete the physical file
                await _fileUploadService.DeleteImageAsync(request.ImageUrl);
                
                return Ok(ApiResponse<bool>.Ok(result, "Image removed successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing image from blog {id}");
                return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Get blogs by status (Admin/Management)
        /// </summary>
        [HttpGet("status/{status}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedBlogResultDTO>>> GetBlogsByStatus(BlogStatus status, [FromQuery] int page = 1, [FromQuery] int pageSize = 6)
        {
            try
            {
                var result = await _blogService.GetBlogsByStatusAsync(status, page, pageSize);
                return Ok(ApiResponse<PagedBlogResultDTO>.Ok(result, "Blogs retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting blogs with status {status}");
                return StatusCode(500, ApiResponse<PagedBlogResultDTO>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Get blogs by user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<PagedBlogResultDTO>>> GetBlogsByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 6)
        {
            try
            {
                var result = await _blogService.GetBlogsByUserAsync(userId, page, pageSize);
                return Ok(ApiResponse<PagedBlogResultDTO>.Ok(result, "Blogs retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting blogs for user {userId}");
                return StatusCode(500, ApiResponse<PagedBlogResultDTO>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Get blogs by category
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<ApiResponse<PagedBlogResultDTO>>> GetBlogsByCategory(int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 6)
        {
            try
            {
                var result = await _blogService.GetBlogsByCategoryAsync(categoryId, page, pageSize);
                return Ok(ApiResponse<PagedBlogResultDTO>.Ok(result, "Blogs retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting blogs for category {categoryId}");
                return StatusCode(500, ApiResponse<PagedBlogResultDTO>.Fail("Internal server error"));
            }
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userPublicId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            
            // Get user by PublicId to find database ID
            var user = await _userRepository.GetByPublicIdAsync(userPublicId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }
            
            return user.Id;
        }
    }

    // Helper DTOs for requests
    public class RejectBlogRequestDTO
    {
        public string RejectionReason { get; set; } = null!;
    }

    public class RemoveImageRequestDTO
    {
        public string ImageUrl { get; set; } = null!;
    }
}
