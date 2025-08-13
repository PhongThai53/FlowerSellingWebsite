using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowerSellingWebsite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// User login endpoint
        /// </summary>
        /// <param name="loginRequest">Login credentials</param>
        /// <returns>Authentication response with JWT token</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponseDTO>> Login([FromBody] LoginRequestDTO loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthenticationResponseDTO
                {
                    Success = false,
                    Message = "Invalid input data",
                });
            }

            var result = await _authService.LoginAsync(loginRequest);
            
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            // Set HTTP-only cookie for additional security (optional)
            if (!string.IsNullOrEmpty(result.Token))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.TokenExpiry
                };
                Response.Cookies.Append("auth_token", result.Token, cookieOptions);
            }

            return Ok(result);
        }

        /// <summary>
        /// User registration endpoint
        /// </summary>
        /// <param name="registerRequest">Registration data</param>
        /// <returns>Authentication response with JWT token</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthenticationResponseDTO>> Register([FromBody] RegisterRequestDTO registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthenticationResponseDTO
                {
                    Success = false,
                    Message = "Invalid input data"
                });
            }

            var result = await _authService.RegisterAsync(registerRequest);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            // Set HTTP-only cookie for additional security (optional)
            if (!string.IsNullOrEmpty(result.Token))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.TokenExpiry
                };
                Response.Cookies.Append("auth_token", result.Token, cookieOptions);
            }

            return Ok(result);
        }

        /// <summary>
        /// User logout endpoint
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // Clear the authentication cookie
            Response.Cookies.Delete("auth_token");
            
            return Ok(ApiResponse<string>.Ok("", "Logged out successfully"));
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        /// <returns>User profile data</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDTO>>> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userPublicId))
            {
                            return Unauthorized(ApiResponse<UserDTO>.Fail("Invalid user token"));
            }

            var userProfile = await _authService.GetUserProfileAsync(userPublicId);
            if (userProfile == null)
            {
                            return NotFound(ApiResponse<UserDTO>.Fail("User not found"));
            }

            return Ok(ApiResponse<UserDTO>.Ok(userProfile, "Profile retrieved successfully"));
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="changePasswordRequest">Password change data</param>
        /// <returns>Success or error response</returns>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<AuthenticationResponseDTO>> ChangePassword([FromBody] ChangePasswordDTO changePasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthenticationResponseDTO
                {
                    Success = false,
                    Message = "Invalid input data"
                });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userPublicId))
            {
                return Unauthorized(new AuthenticationResponseDTO
                {
                    Success = false,
                    Message = "Invalid user token"
                });
            }

            var result = await _authService.ChangePasswordAsync(userPublicId, changePasswordRequest);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Forgot password endpoint
        /// </summary>
        /// <param name="forgotPasswordRequest">Email for password reset</param>
        /// <returns>Success message</returns>
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                            return BadRequest(ApiResponse<string>.Fail("Invalid email address"));
            }

            var result = await _authService.ForgotPasswordAsync(forgotPasswordRequest);
            return Ok(result);
        }

        /// <summary>
        /// Reset password endpoint
        /// </summary>
        /// <param name="resetPasswordRequest">Reset password data</param>
        /// <returns>Success or error response</returns>
        [HttpPost("reset-password")]
        public async Task<ActionResult<AuthenticationResponseDTO>> ResetPassword([FromBody] ResetPasswordDTO resetPasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthenticationResponseDTO
                {
                    Success = false,
                    Message = "Invalid input data"
                });
            }

            var result = await _authService.ResetPasswordAsync(resetPasswordRequest);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Validate JWT token endpoint
        /// </summary>
        /// <returns>Token validation result</returns>
        [HttpGet("validate-token")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            // If we reach here, the token is valid (handled by [Authorize] attribute)
            return Ok(ApiResponse<string>.Ok("", "Token is valid"));
        }
    }
}
