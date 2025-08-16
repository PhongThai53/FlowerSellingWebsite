using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.DTOs.Comment;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowerSellingWebsite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentController> _logger;

        public CommentController(ICommentService commentService, ILogger<CommentController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint to verify the controller is working
        /// </summary>
        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            return Ok("Comment Controller is working!");
        }

        /// <summary>
        /// Get comment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CommentDTO>>> GetCommentById(int id)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id);
                return Ok(ApiResponse<CommentDTO>.Ok(comment, "Comment retrieved successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<CommentDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting comment with ID {id}");
                return StatusCode(500, ApiResponse<CommentDTO>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Get all comments for a specific blog (with nested structure)
        /// </summary>
        [HttpGet("blog/{blogId}")]
        public async Task<ActionResult<ApiResponse<List<CommentDTO>>>> GetCommentsByBlogId(int blogId)
        {
            try
            {
                var comments = await _commentService.GetCommentsByBlogIdAsync(blogId);
                return Ok(ApiResponse<List<CommentDTO>>.Ok(comments, "Comments retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting comments for blog {blogId}");
                return StatusCode(500, ApiResponse<List<CommentDTO>>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Get replies for a specific comment
        /// </summary>
        [HttpGet("parent/{parentId}/replies")]
        public async Task<ActionResult<ApiResponse<List<CommentDTO>>>> GetCommentsByParentId(int parentId)
        {
            try
            {
                var comments = await _commentService.GetCommentsByParentIdAsync(parentId);
                return Ok(ApiResponse<List<CommentDTO>>.Ok(comments, "Replies retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting replies for comment {parentId}");
                return StatusCode(500, ApiResponse<List<CommentDTO>>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Get all comments by a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<CommentDTO>>>> GetCommentsByUserId(int userId)
        {
            try
            {
                var comments = await _commentService.GetCommentsByUserIdAsync(userId);
                return Ok(ApiResponse<List<CommentDTO>>.Ok(comments, "User comments retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting comments for user {userId}");
                return StatusCode(500, ApiResponse<List<CommentDTO>>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Create a new comment or reply
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CommentDTO>>> CreateComment([FromBody] CreateCommentDTO createCommentDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<CommentDTO>.Fail("Invalid input data"));
                }

                var userId = GetCurrentUserId();
                var createdComment = await _commentService.CreateCommentAsync(createCommentDTO, userId);
                
                return CreatedAtAction(nameof(GetCommentById), new { id = createdComment.Id }, 
                    ApiResponse<CommentDTO>.Ok(createdComment, "Comment created successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<CommentDTO>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CommentDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                return StatusCode(500, ApiResponse<CommentDTO>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Update an existing comment
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CommentDTO>>> UpdateComment(int id, [FromBody] UpdateCommentDTO updateCommentDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<CommentDTO>.Fail("Invalid input data"));
                }

                var userId = GetCurrentUserId();
                var updatedComment = await _commentService.UpdateCommentAsync(id, updateCommentDTO, userId);
                
                return Ok(ApiResponse<CommentDTO>.Ok(updatedComment, "Comment updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<CommentDTO>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating comment with ID {id}");
                return StatusCode(500, ApiResponse<CommentDTO>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Delete a comment
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteComment(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _commentService.DeleteCommentAsync(id, userId);
                
                return Ok(ApiResponse<bool>.Ok(result, "Comment deleted successfully"));
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
                _logger.LogError(ex, $"Error deleting comment with ID {id}");
                return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Hide a comment (Blog owner or Admin)
        /// </summary>
        [HttpPost("{id}/hide")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> HideComment(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _commentService.HideCommentAsync(id, userId);
                
                return Ok(ApiResponse<bool>.Ok(result, "Comment hidden successfully"));
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
                _logger.LogError(ex, $"Error hiding comment with ID {id}");
                return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
            }
        }

        /// <summary>
        /// Show a hidden comment (Blog owner or Admin)
        /// </summary>
        [HttpPost("{id}/show")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> ShowComment(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _commentService.ShowCommentAsync(id, userId);
                
                return Ok(ApiResponse<bool>.Ok(result, "Comment shown successfully"));
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
                _logger.LogError(ex, $"Error showing comment with ID {id}");
                return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }
    }
}
