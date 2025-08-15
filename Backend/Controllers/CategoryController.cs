using FlowerSellingWebsite.Models.DTOs.Category;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowerSellingWebsite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        /// <summary>
        /// Test endpoint to verify the controller is working
        /// </summary>
        /// <returns>A simple test message</returns>
        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            return Ok("Category Controller is working!");
        }

        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>List of all categories</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get a category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting category with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="categoryDTO">Category data</param>
        /// <returns>Created category</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryDTO>> CreateCategory([FromBody] CreateCategoryDTO categoryDTO)
        {
            try
            {
                var createdCategory = await _categoryService.CreateCategoryAsync(categoryDTO);
                return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update a category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="categoryDTO">Updated category data</param>
        /// <returns>Updated category</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryDTO>> UpdateCategory(int id, [FromBody] UpdateCategoryDTO categoryDTO)
        {
            try
            {
                var updatedCategory = await _categoryService.UpdateCategoryAsync(id, categoryDTO);
                return Ok(updatedCategory);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found for update");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating category with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found for deletion");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting category with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get categories by status (active/inactive)
        /// </summary>
        /// <param name="isActive">Status filter</param>
        /// <returns>List of filtered categories</returns>
        [HttpGet("status")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategoriesByStatus([FromQuery] bool isActive = true)
        {
            try
            {
                var categories = await _categoryService.GetCategoriesByStatusAsync(isActive);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting categories with status isActive={isActive}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update category status (activate/deactivate)
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="isActive">New status</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateCategoryStatus(int id, [FromQuery] bool isActive)
        {
            try
            {
                await _categoryService.UpdateCategoryStatusAsync(id, isActive);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found for status update");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for category with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
