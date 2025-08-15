using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace FlowerSellingWebsite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPasswordResetService _passwordResetService;

        public AuthController(IUserService userService, ILogger<AuthController> logger, IConfiguration configuration, IPasswordResetService passwordResetService)
        {
            _userService = userService;
            _logger = logger;
            _configuration = configuration;
            _passwordResetService = passwordResetService;
        }

   
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login([FromBody] LoginRequestDTO loginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<LoginResponseDTO>.Fail("Invalid input data"));
                }

                var result = await _userService.LoginAsync(loginRequest);

                // Set HTTP-only cookie for additional security (optional)
                if (!string.IsNullOrEmpty(result.Token))
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = result.ExpiresAt
                    };
                    Response.Cookies.Append("auth_token", result.Token, cookieOptions);
                }

                return Ok(ApiResponse<LoginResponseDTO>.Ok(result, "Login successful"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<LoginResponseDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", loginRequest.Email);
                return StatusCode(500, ApiResponse<LoginResponseDTO>.Fail("An error occurred during login. Please try again."));
            }
        }


        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Register([FromBody] RegisterRequestDTO registerRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(ApiResponse<UserDTO>.Fail("Invalid input data", errors));
                }

                var result = await _userService.RegisterAsync(registerRequest);

                return Ok(ApiResponse<UserDTO>.Ok(result, "Registration successful"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<UserDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", registerRequest.Email);
                return StatusCode(500, ApiResponse<UserDTO>.Fail("An error occurred during registration. Please try again."));
            }
        }


        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // Clear the authentication cookie
            Response.Cookies.Delete("auth_token");
            
            return Ok(ApiResponse<string>.Ok("", "Logged out successfully"));
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDTO>>> GetProfile()
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
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, ApiResponse<UserDTO>.Fail("An error occurred while retrieving profile"));
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordRequestDTO changePasswordRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<string>.Fail("Invalid input data"));
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userPublicId))
                {
                    return Unauthorized(ApiResponse<string>.Fail("Invalid user token"));
                }

                var result = await _userService.ChangePasswordAsync(userPublicId, changePasswordRequest);
                
                if (result)
                {
                    return Ok(ApiResponse<string>.Ok("", "Password changed successfully"));
                }

                return BadRequest(ApiResponse<string>.Fail("Failed to change password"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<string>.Fail(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while changing password"));
            }
        }


        [HttpGet("validate-token")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            return Ok(ApiResponse<string>.Ok("", "Token is valid"));
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    // Redirect to a failed page with an error message
                    return Redirect($"{_configuration["FrontendUrl"]}/html/auth/verification-failed.html?error=invalid_token");
                }

                var result = await _userService.VerifyEmailAsync(token);

                if (result)
                {
                    // Redirect to a success page
                    return Redirect($"{_configuration["FrontendUrl"]}/html/auth/verification-success.html");
                }
                else
                {
                    // Redirect to a failed page
                    return Redirect($"{_configuration["FrontendUrl"]}/html/auth/verification-failed.html?error=verification_failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification");
                // Redirect to a generic error page
                return Redirect($"{_configuration["FrontendUrl"]}/html/auth/verification-failed.html?error=server_error");
            }
        }

        [HttpPost("resend-verification")]
        public async Task<ActionResult<ApiResponse<string>>> ResendVerificationEmail([FromBody] ResendVerificationRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<string>.Fail("Invalid input data"));
                }

                var result = await _userService.ResendVerificationEmailAsync(request.Email);

                if (result)
                {
                    return Ok(ApiResponse<string>.Ok("", "Verification email sent successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<string>.Fail("Failed to resend verification email. Please check if the email is valid and pending verification."));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification email for: {Email}", request.Email);
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while resending verification email"));
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDto)
        {
            try
            {
                await _userService.SendPasswordResetLinkAsync(forgotPasswordDto.Email);
                var message = "<div class='alert alert-success'>If an account with that email exists, a password reset link has been sent.</div>";
                return Content(message, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset link for email {Email}", forgotPasswordDto.Email);
                var message = "<div class='alert alert-danger'>An unexpected error occurred. Please try again later.</div>";
                return Content(message, "text/html");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDto)
        {
            try
            {
                var success = await _userService.ResetPasswordAsync(resetPasswordDto.Token, resetPasswordDto.NewPassword);
                if (success)
                {
                    var message = "<div class='alert alert-success'>Password has been reset successfully. <a href='/html/auth/login-register.html'>Click here to login</a>.</div>";
                    return Content(message, "text/html");
                }
                var errorMessage = "<div class='alert alert-danger'>Invalid or expired token. Please try resetting your password again.</div>";
                return Content(errorMessage, "text/html");
            }
            catch (InvalidOperationException ex)
            {
                var errorMessage = $"<div class='alert alert-danger'>{ex.Message}</div>";
                return Content(errorMessage, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password.");
                var errorMessage = "<div class='alert alert-danger'>An unexpected error occurred. Please try again later.</div>";
                return Content(errorMessage, "text/html");
            }
        }
    }
}