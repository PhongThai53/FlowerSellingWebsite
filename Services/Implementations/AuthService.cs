using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using ProjectGreenLens.Services.Interfaces;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IAuthRepository authRepository,
            IJwtService jwtService,
            ILogger<AuthService> logger)
        {
            _authRepository = authRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<AuthenticationResponseDTO> LoginAsync(LoginRequestDTO loginRequest)
        {
            try
            {
                // Find user by username or email
                var user = await _authRepository.GetUserByUsernameOrEmailAsync(loginRequest.EmailOrUsername);
                if (user == null)
                {
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = "Invalid username/email or password"
                    };
                }

                // Verify password
                if (!await _authRepository.VerifyPasswordAsync(user, loginRequest.Password))
                {
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = "Invalid username/email or password"
                    };
                }

                // Generate JWT token
                var token = await _jwtService.GenerateTokenAsync(user.PublicId.ToString(), user.Role.RoleName);
                var tokenExpiry = DateTime.UtcNow.AddMinutes(120); // Should match JWT settings

                return new AuthenticationResponseDTO
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    TokenExpiry = tokenExpiry,
                    User = MapToUserDTO(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {EmailOrUsername}", loginRequest.EmailOrUsername);
                return new AuthenticationResponseDTO
                {
                    Success = false,
                    Message = "An error occurred during login. Please try again."
                };
            }
        }

        public async Task<AuthenticationResponseDTO> RegisterAsync(RegisterRequestDTO registerRequest)
        {
            _logger.LogInformation("Starting registration process for username: {UserName}, email: {Email}", 
                registerRequest.UserName, registerRequest.Email);

            try
            {
                // Step 1: Check if username already exists
                _logger.LogDebug("Checking if username {UserName} already exists...", registerRequest.UserName);
                bool usernameExists = false;
                try
                {
                    usernameExists = await _authRepository.IsUsernameExistsAsync(registerRequest.UserName);
                    _logger.LogDebug("Username check completed. Exists: {Exists}", usernameExists);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to check username availability for {UserName}", registerRequest.UserName);
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = "Failed to verify username availability. Please try again.",
                        Errors = new List<string> { "Username validation error", ex.Message }
                    };
                }

                if (usernameExists)
                {
                    _logger.LogWarning("Registration failed: Username {UserName} is already taken", registerRequest.UserName);
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = "Username is already taken"
                    };
                }

                // Step 2: Check if email already exists
                _logger.LogDebug("Checking if email {Email} already exists...", registerRequest.Email);
                bool emailExists = false;
                try
                {
                    emailExists = await _authRepository.IsEmailExistsAsync(registerRequest.Email);
                    _logger.LogDebug("Email check completed. Exists: {Exists}", emailExists);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to check email availability for {Email}", registerRequest.Email);
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = "Failed to verify email availability. Please try again.",
                        Errors = new List<string> { "Email validation error", ex.Message }
                    };
                }

                if (emailExists)
                {
                    _logger.LogWarning("Registration failed: Email {Email} is already registered", registerRequest.Email);
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = "Email is already registered"
                    };
                }

                // Step 3: Get Customer role
                _logger.LogDebug("Retrieving Customer role from database...");
                Role? customerRole = null;
                try
                {
                    customerRole = await _authRepository.GetRoleByNameAsync("Customer");
                    if (customerRole != null)
                    {
                        _logger.LogDebug("Customer role retrieved successfully. RoleId: {RoleId}", customerRole.Id);
                    }
                    else
                    {
                        _logger.LogError("Customer role not found in database");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retrieve Customer role from database");
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = "Failed to retrieve role information. Please contact administrator.",
                        Errors = new List<string> { "Role retrieval error", ex.Message }
                    };
                }

                if (customerRole == null)
                {
                    _logger.LogError("Registration failed: Customer role not found in the system");
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = "Customer role not found. Please contact administrator."
                    };
                }

                // Step 4: Hash password
                _logger.LogDebug("Hashing password for user {UserName}...", registerRequest.UserName);
                string passwordHash;
                try
                {
                    passwordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);
                    _logger.LogDebug("Password hashed successfully for user {UserName}", registerRequest.UserName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to hash password for user {UserName}", registerRequest.UserName);
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = "Failed to process password. Please try again.",
                        Errors = new List<string> { "Password processing error", ex.Message }
                    };
                }

                // Step 5: Create new user object
                _logger.LogDebug("Creating user object for {UserName}...", registerRequest.UserName);
                var newUser = new User
                {
                    UserName = registerRequest.UserName,
                    PasswordHash = passwordHash,
                    FullName = registerRequest.FullName,
                    Email = registerRequest.Email,
                    Phone = registerRequest.Phone,
                    Address = registerRequest.Address,
                    RoleId = customerRole.Id,
                    IsCustomer = true,
                    IsSupplier = false
                };

                // Step 6: Save user to database
                _logger.LogInformation("Attempting to save new user {UserName} to database...", registerRequest.UserName);
                User? createdUser = null;
                try
                {
                    createdUser = await _authRepository.CreateUserAsync(newUser);
                    
                    if (createdUser != null)
                    {
                        _logger.LogInformation("User {UserName} created successfully with PublicId: {PublicId}", 
                            createdUser.UserName, createdUser.PublicId);
                    }
                    else
                    {
                        _logger.LogError("CreateUserAsync returned null for user {UserName}", registerRequest.UserName);
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database update error while creating user {UserName}. " +
                        "Inner exception: {InnerException}", 
                        registerRequest.UserName, 
                        dbEx.InnerException?.Message ?? "None");
                    
                    // Provide more specific error messages based on common database errors
                    string errorMessage = "A database error occurred during registration.";
                    var errors = new List<string>();
                    
                    if (dbEx.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        errorMessage = "User information conflicts with existing data.";
                        errors.Add("Duplicate key violation");
                    }
                    else if (dbEx.InnerException?.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        errorMessage = "Invalid role assignment. Please contact administrator.";
                        errors.Add("Foreign key constraint violation");
                    }
                    
                    errors.Add(dbEx.InnerException?.Message ?? dbEx.Message);
                    
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = errorMessage,
                        Errors = errors
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while creating user {UserName} in database", 
                        registerRequest.UserName);
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = "Failed to create user account. Please try again.",
                        Errors = new List<string> { "User creation error", ex.Message }
                    };
                }
                
                if (createdUser == null || createdUser.Role == null)
                {
                    _logger.LogError("User creation validation failed: CreatedUser is {UserNull}, Role is {RoleNull}",
                        createdUser == null ? "null" : "not null",
                        createdUser?.Role == null ? "null" : "not null");
                    
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = "Failed to create user account or assign a role. User or role data is missing."
                    };
                }

                // Step 7: Generate JWT token for auto-login
                _logger.LogDebug("Generating JWT token for user {UserName}...", createdUser.UserName);
                string token;
                DateTime tokenExpiry;
                try
                {
                    token = await _jwtService.GenerateTokenAsync(createdUser.PublicId.ToString(), createdUser.Role.RoleName);
                    tokenExpiry = DateTime.UtcNow.AddMinutes(120);
                    _logger.LogDebug("JWT token generated successfully for user {UserName}", createdUser.UserName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate JWT token for user {UserName}, but registration was successful", 
                        createdUser.UserName);
                    
                    // Registration was successful, but token generation failed
                    // User can still log in manually
                    return new AuthenticationResponseDTO
                    {
                        Success = true,
                        Message = "Registration successful, but auto-login failed. Please log in manually.",
                        User = MapToUserDTO(createdUser)
                    };
                }

                _logger.LogInformation("Registration completed successfully for user {UserName} with PublicId: {PublicId}",
                    createdUser.UserName, createdUser.PublicId);

                return new AuthenticationResponseDTO
                {
                    Success = true,
                    Message = "Registration successful",
                    Token = token,
                    TokenExpiry = tokenExpiry,
                    User = MapToUserDTO(createdUser)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration process for user: {UserName}. " +
                    "Exception type: {ExceptionType}, Message: {ExceptionMessage}, " +
                    "StackTrace: {StackTrace}",
                    registerRequest.UserName,
                    ex.GetType().Name,
                    ex.Message,
                    ex.StackTrace);
                
                return new AuthenticationResponseDTO
                {
                    Success = false,
                    Message = "An unexpected error occurred during registration. Please try again.",
                    Errors = new List<string> { $"Error type: {ex.GetType().Name}", ex.Message }
                };
            }
        }

        public async Task<AuthenticationResponseDTO> ChangePasswordAsync(Guid userPublicId, ChangePasswordDTO changePasswordRequest)
        {
            try
            {
                var user = await _authRepository.GetUserByPublicIdAsync(userPublicId);
                if (user == null)
                {
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Verify current password
                if (!await _authRepository.VerifyPasswordAsync(user, changePasswordRequest.CurrentPassword))
                {
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = "Current password is incorrect"
                    };
                }

                // Hash new password
                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordRequest.NewPassword);

                // Update password
                if (await _authRepository.UpdatePasswordAsync(user, newPasswordHash))
                {
                    return new AuthenticationResponseDTO
                    {
                        Success = true,
                        Message = "Password changed successfully"
                    };
                }

                return new AuthenticationResponseDTO
                {
                    Success = false,
                    Message = "Failed to change password"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserPublicId}", userPublicId);
                return new AuthenticationResponseDTO
                {
                    Success = false,
                    Message = "An error occurred while changing password. Please try again."
                };
            }
        }

        public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordRequest)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(forgotPasswordRequest.Email);
                if (user == null)
                {
                                    // Don't reveal if email exists or not for security reasons
                return ApiResponse<string>.Ok("", "If the email exists in our system, a password reset link has been sent.");
                }

                // TODO: Implement email service to send password reset email
                // For now, just return success message
                _logger.LogInformation("Password reset requested for email: {Email}", forgotPasswordRequest.Email);

                return ApiResponse<string>.Ok("", "If the email exists in our system, a password reset link has been sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for email: {Email}", forgotPasswordRequest.Email);
                return ApiResponse<string>.Fail("An error occurred. Please try again.");
            }
        }

        public async Task<AuthenticationResponseDTO> ResetPasswordAsync(ResetPasswordDTO resetPasswordRequest)
        {
            try
            {
                // TODO: Implement proper token validation for password reset
                // For now, just find user by email
                var user = await _authRepository.GetUserByEmailAsync(resetPasswordRequest.Email);
                if (user == null)
                {
                    return new AuthenticationResponseDTO
                    {
                        Success = false,
                        Message = "Invalid reset token or email"
                    };
                }

                // Hash new password
                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordRequest.NewPassword);

                // Update password
                if (await _authRepository.UpdatePasswordAsync(user, newPasswordHash))
                {
                    return new AuthenticationResponseDTO
                    {
                        Success = true,
                        Message = "Password reset successful"
                    };
                }

                return new AuthenticationResponseDTO
                {
                    Success = false,
                    Message = "Failed to reset password"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for email: {Email}", resetPasswordRequest.Email);
                return new AuthenticationResponseDTO
                {
                    Success = false,
                    Message = "An error occurred while resetting password. Please try again."
                };
            }
        }

        public async Task<UserDTO?> GetUserProfileAsync(Guid userPublicId)
        {
            try
            {
                var user = await _authRepository.GetUserByPublicIdAsync(userPublicId);
                return user != null ? MapToUserDTO(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for: {UserPublicId}", userPublicId);
                return null;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var principal = await _jwtService.ValidateTokenAsync(token);
                return principal != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }

        private static UserDTO MapToUserDTO(User user)
        {
            return new UserDTO
            {
                PublicId = user.PublicId,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role.RoleName,
                IsCustomer = user.IsCustomer,
                IsSupplier = user.IsSupplier,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
