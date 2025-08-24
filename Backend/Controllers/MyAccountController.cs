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
                // Additional server-side validation beyond Data Annotations
                var validationErrors = ValidateUpdateRequest(updateRequest);
                if (validationErrors.Any())
                {
                    return BadRequest(ApiResponse<UserDTO>.Fail(string.Join("; ", validationErrors)));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<UserDTO>.Fail(string.Join("; ", errors)));
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
                    FullName = updateRequest.FullName?.Trim(),
                    UserName = updateRequest.UserName?.Trim(),
                    PhoneNumber = updateRequest.Phone?.Trim(),
                    Address = !string.IsNullOrWhiteSpace(updateRequest.Address) 
                        ? updateRequest.Address.Trim() 
                        : null
                };

                var updatedUser = await _userService.UpdateUserAsync(userPublicId, updateUserRequest);
                
                // Log successful profile update for security monitoring
                _logger.LogInformation("User {UserId} updated their profile successfully", userPublicId);
                
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
                // Additional server-side validation for password change
                var passwordValidationErrors = ValidatePasswordChangeRequest(changePasswordRequest);
                if (passwordValidationErrors.Any())
                {
                    return BadRequest(ApiResponse<string>.Fail(string.Join("; ", passwordValidationErrors)));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<string>.Fail(string.Join("; ", errors)));
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
                    // Log successful password change for security monitoring
                    _logger.LogInformation("User {UserId} changed their password successfully", userPublicId);
                    
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

        /// <summary>
        /// Additional server-side validation for update request
        /// </summary>
        /// <param name="request">Update request to validate</param>
        /// <returns>List of validation errors</returns>
        private List<string> ValidateUpdateRequest(UpdateAccountRequestDTO request)
        {
            var errors = new List<string>();

            // Required field validation with detailed checks
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                errors.Add("Full name is required and cannot be blank or contain only spaces");
            }
            else if (request.FullName.Trim().Length < 2)
            {
                errors.Add("Full name must be at least 2 characters long");
            }

            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                errors.Add("Username is required and cannot be blank or contain only spaces");
            }
            else if (request.UserName.Trim().Length < 3)
            {
                errors.Add("Username must be at least 3 characters long");
            }

            if (string.IsNullOrWhiteSpace(request.Phone))
            {
                errors.Add("Phone number is required and cannot be blank or contain only spaces");
            }

            // Length validation to prevent DB overflow
            if (request.FullName?.Length > 100)
            {
                errors.Add("Full name cannot exceed 100 characters");
            }

            if (request.UserName?.Length > 50)
            {
                errors.Add("Username cannot exceed 50 characters");
            }

            if (request.Phone?.Length > 20)
            {
                errors.Add("Phone number cannot exceed 20 characters");
            }

            if (request.Address?.Length > 500)
            {
                errors.Add("Address cannot exceed 500 characters");
            }

            // Phone format validation
            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                var cleanPhone = System.Text.RegularExpressions.Regex.Replace(request.Phone, @"[\s\-\(\)\+]", "");
                if (!System.Text.RegularExpressions.Regex.IsMatch(cleanPhone, @"^[0-9]{10,15}$"))
                {
                    errors.Add("Please enter a valid phone number (10-15 digits)");
                }
            }

            return errors;
        }

        /// <summary>
        /// Additional server-side validation for password change request
        /// </summary>
        /// <param name="request">Password change request to validate</param>
        /// <returns>List of validation errors</returns>
        private List<string> ValidatePasswordChangeRequest(ChangePasswordRequestDTO request)
        {
            var errors = new List<string>();

            // Required field validation
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                errors.Add("Current password is required");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                errors.Add("New password is required");
            }
            else
            {
                // Password strength validation
                if (request.NewPassword.Length < 8)
                {
                    errors.Add("New password must be at least 8 characters long");
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(request.NewPassword, @"[a-z]"))
                {
                    errors.Add("New password must contain at least one lowercase letter");
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(request.NewPassword, @"[A-Z]"))
                {
                    errors.Add("New password must contain at least one uppercase letter");
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(request.NewPassword, @"\d"))
                {
                    errors.Add("New password must contain at least one number");
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(request.NewPassword, @"[\W_]"))
                {
                    errors.Add("New password must contain at least one special character");
                }
            }

            if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
            {
                errors.Add("Confirm password is required");
            }
            else if (request.NewPassword != request.ConfirmPassword)
            {
                errors.Add("New password and confirm password do not match");
            }

            return errors;
        }
    }
}

