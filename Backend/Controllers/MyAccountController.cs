using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowerSellingWebsite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MyAccountController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<MyAccountController> _logger;

        public MyAccountController(IUserService userService, ILogger<MyAccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user account information
        /// </summary>
        /// <returns>Current user details</returns>
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> GetCurrentUserProfile()
        {
            try
            {
                var userProfile = await _userService.GetCurrentUserAsync();
                return Ok(ApiResponse<UserDTO>.Ok(userProfile, "Profile retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<UserDTO>.Fail(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<UserDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user profile");
                return StatusCode(500, ApiResponse<UserDTO>.Fail("An error occurred while retrieving profile"));
            }
        }

        /// <summary>
        /// Update current user account information
        /// </summary>
        /// <param name="updateRequest">Account update request</param>
        /// <returns>Updated user profile</returns>
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> UpdateCurrentUserProfile([FromBody] UpdateAccountRequestDTO updateRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var firstError = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .FirstOrDefault();

                    if (firstError != null)
                    {
                        return BadRequest(ApiResponse<UserDTO>.Fail(firstError));
                    }
                    
                    return BadRequest(ApiResponse<UserDTO>.Fail("Invalid input data"));
                }

                // Get current user ID from token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userPublicId))
                {
                    return Unauthorized(ApiResponse<UserDTO>.Fail("Invalid user token"));
                }

                // Create UpdateUserRequestDTO from UpdateAccountRequestDTO
                var updateUserRequest = new UpdateUserRequestDTO
                {
                    FullName = updateRequest.FullName,
                    PhoneNumber = updateRequest.Phone,
                    Address = updateRequest.Address
                };

                var updatedUser = await _userService.UpdateUserAsync(userPublicId, updateUserRequest);
                return Ok(ApiResponse<UserDTO>.Ok(updatedUser, "Profile updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<UserDTO>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<UserDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, ApiResponse<UserDTO>.Fail("An error occurred while updating profile"));
            }
        }

        /// <summary>
        /// Change current user password
        /// </summary>
        /// <param name="changePasswordRequest">Password change request</param>
        /// <returns>Success response</returns>
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordRequestDTO changePasswordRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var firstError = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .FirstOrDefault();

                    if (firstError != null)
                    {
                        return BadRequest(ApiResponse<string>.Fail(firstError));
                    }
                    
                    return BadRequest(ApiResponse<string>.Fail("Invalid input data"));
                }

                // Get current user ID from token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userPublicId))
                {
                    return Unauthorized(ApiResponse<string>.Fail("Invalid user token"));
                }

                var result = await _userService.ChangePasswordAsync(userPublicId, changePasswordRequest);
                if (result)
                {
                    return Ok(ApiResponse<string>.Ok("", "Password changed successfully. Please log in again."));
                }
                else
                {
                    return BadRequest(ApiResponse<string>.Fail("Failed to change password"));
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<string>.Fail(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while changing password"));
            }
        }

        /// <summary>
        /// Get current user address
        /// </summary>
        /// <returns>Current user address</returns>
        [HttpGet("address")]
        public async Task<ActionResult<ApiResponse<object>>> GetCurrentUserAddress()
        {
            try
            {
                var userProfile = await _userService.GetCurrentUserAsync();
                var addressInfo = new
                {
                    Address = userProfile.Address ?? string.Empty,
                    FullName = userProfile.FullName,
                    Phone = userProfile.PhoneNumber ?? string.Empty
                };
                
                return Ok(ApiResponse<object>.Ok(addressInfo, "Address retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.Fail(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user address");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while retrieving address"));
            }
        }
    }
}

