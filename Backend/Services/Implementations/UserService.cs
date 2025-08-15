using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using ProjectGreenLens.Services.Interfaces;
using System.Security.Claims;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<UserService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly IEmailVerificationService _verificationService;
        private readonly IPendingUserService _pendingUserService;
        private readonly IPasswordResetService _passwordResetService;

        public UserService(
            IUserRepository userRepository,
            IJwtService jwtService,
            ILogger<UserService> logger,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            IEmailVerificationService verificationService,
            IPendingUserService pendingUserService,
            IPasswordResetService passwordResetService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _verificationService = verificationService;
            _pendingUserService = pendingUserService;
            _passwordResetService = passwordResetService;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            _logger.LogInformation("Starting login process for email: {Email}", request.Email);

            try
            {
                // Find user by email
                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: Users not found for email {Email}", request.Email);
                    throw new UnauthorizedAccessException("Invalid email or password");
                }

                // Check if user has pending email verification
                if (_verificationService.IsEmailPendingVerification(request.Email))
                {
                    _logger.LogWarning("Login failed: Email verification pending for {Email}", request.Email);
                    throw new UnauthorizedAccessException("Please verify your email address before logging in. Check your inbox for the verification link.");
                }

                // Verify password
                if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Invalid password for email {Email}", request.Email);
                    throw new UnauthorizedAccessException("Invalid email or password");
                }

                // Generate JWT token
                var token = await _jwtService.GenerateTokenAsync(user.PublicId.ToString(), user.Role.RoleName);
                var expiresAt = DateTime.UtcNow.AddMinutes(120); // Should match JWT settings

                _logger.LogInformation("Login successful for user {Email} with PublicId: {PublicId}", user.Email, user.PublicId);

                return new LoginResponseDTO
                {
                    Token = token,
                    User = MapToUserDTO(user),
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                throw;
            }
        }

        public async Task<UserDTO> RegisterAsync(RegisterRequestDTO request)
        {
            _logger.LogInformation("Starting registration process for username: {UserName}, email: {Email}",
                request.UserName, request.Email);

            try
            {
                // Check if username already exists
                if (await _userRepository.UsernameExistsAsync(request.UserName))
                {
                    _logger.LogWarning("Registration failed: Username {UserName} already exists", request.UserName);
                    throw new InvalidOperationException("Username is already taken");
                }

                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(request.Email))
                {
                    _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
                    throw new InvalidOperationException("Email is already registered");
                }

                // Check if email is already pending verification
                if (_verificationService.IsEmailPendingVerification(request.Email))
                {
                    _logger.LogWarning("Registration failed: Email {Email} is already pending verification", request.Email);
                    throw new InvalidOperationException("Email verification is already pending. Please check your inbox or request a new verification email.");
                }

                // Store pending user data
                _pendingUserService.StorePendingUser(request.Email, request);

                // Generate verification token
                var verificationToken = _verificationService.GenerateVerificationToken(request.Email);

                // Send verification email
                await _emailService.SendEmailVerificationAsync(request.Email, request.FullName, verificationToken);

                _logger.LogInformation("Registration initiated for user {UserName} ({Email}). Verification email sent.", 
                    request.UserName, request.Email);

                // Return a temporary user DTO indicating verification is required
                return new UserDTO
                {
                    PublicId = Guid.NewGuid(), // Temporary ID
                    FullName = request.FullName,
                    UserName = request.UserName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    RoleName = "Users",
                    IsCustomer = true,
                    IsSupplier = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for username: {UserName}, email: {Email}",
                    request.UserName, request.Email);
                throw;
            }
        }

        public async Task<UserDTO> GetUserByIdAsync(Guid publicId)
        {
            try
            {
                var user = await _userRepository.GetByPublicIdAsync(publicId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"Users with ID {publicId} not found");
                }

                return MapToUserDTO(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {PublicId}", publicId);
                throw;
            }
        }

        public async Task<UserDTO> GetCurrentUserAsync()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userPublicId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }

            return await GetUserByIdAsync(userPublicId);
        }

        public async Task<UserDTO> CreateUserAsync(CreateUserRequestDTO request)
        {
            _logger.LogInformation("Creating user for email: {Email}", request.Email);

            try
            {
                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(request.Email))
                {
                    throw new InvalidOperationException("Email is already registered");
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Get role from database
                var role = await _userRepository.GetRoleByNameAsync(request.RoleName);
                if (role == null)
                {
                    throw new InvalidOperationException($"Role '{request.RoleName}' not found");
                }

                // Create new user
                var newUser = new Users
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Phone = request.PhoneNumber,
                    Address = request.Address,
                    RoleId = role.Id,
                    //IsCustomer = request.RoleName.Equals("Customer", StringComparison.OrdinalIgnoreCase),
                    //IsSupplier = request.RoleName.Equals("Supplier", StringComparison.OrdinalIgnoreCase)
                };

                await _userRepository.AddAsync(newUser);

                var createdUser = await _userRepository.GetByPublicIdAsync(newUser.PublicId);
                if (createdUser == null)
                {
                    throw new InvalidOperationException("Failed to create user account");
                }

                return MapToUserDTO(createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user for email: {Email}", request.Email);
                throw;
            }
        }

        public async Task<UserDTO> UpdateUserAsync(Guid publicId, UpdateUserRequestDTO request)
        {
            try
            {
                var user = await _userRepository.GetByPublicIdAsync(publicId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"Users with ID {publicId} not found");
                }

                // Update user properties
                user.FullName = request.FullName ?? user.FullName;
                user.Phone = request.PhoneNumber ?? user.Phone;
                user.Address = request.Address ?? user.Address;

                await _userRepository.UpdateAsync(user);

                return MapToUserDTO(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {PublicId}", publicId);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(Guid publicId)
        {
            try
            {
                var user = await _userRepository.GetByPublicIdAsync(publicId);
                if (user == null)
                {
                    return false;
                }

                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {PublicId}", publicId);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(Guid publicId, ChangePasswordRequestDTO request)
        {
            try
            {
                var user = await _userRepository.GetByPublicIdAsync(publicId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"Users with ID {publicId} not found");
                }

                if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Current password is incorrect");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                await _userRepository.UpdateAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {PublicId}", publicId);
                throw;
            }
        }

        public async Task<IEnumerable<UserDTO>> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null, string? role = null)
        {
            try
            {
                var users = await _userRepository.GetUsersAsync(page, pageSize, search, role);
                return users.Select(MapToUserDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users list");
                throw;
            }
        }

        public async Task<bool> ActivateUserAsync(Guid publicId)
        {
            try
            {
                var user = await _userRepository.GetByPublicIdAsync(publicId);
                if (user == null)
                {
                    return false;
                }

                // Assuming there's an IsActive property or similar
                await _userRepository.UpdateAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user: {PublicId}", publicId);
                throw;
            }
        }

        public async Task<bool> DeactivateUserAsync(Guid publicId)
        {
            try
            {
                var user = await _userRepository.GetByPublicIdAsync(publicId);
                if (user == null)
                {
                    return false;
                }

                // Assuming there's an IsActive property or similar
                await _userRepository.UpdateAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user: {PublicId}", publicId);
                throw;
            }
        }

        private static UserDTO MapToUserDTO(Users user)
        {
            return new UserDTO
            {
                PublicId = user.PublicId,
                FullName = user.FullName ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.Phone,
                Address = user.Address,
                IsActive = !user.IsDeleted,
                RoleName = user.Role?.RoleName ?? string.Empty,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            _logger.LogInformation("Starting email verification process for token: {TokenPrefix}...", token[..Math.Min(8, token.Length)]);

            try
            {
                // Validate the verification token
                if (!_verificationService.ValidateVerificationToken(token, out string email))
                {
                    _logger.LogWarning("Email verification failed: Invalid or expired token");
                    return false;
                }

                // Get pending user data
                var pendingUserData = _pendingUserService.GetPendingUser(email);
                if (pendingUserData == null)
                {
                    _logger.LogWarning("Email verification failed: No pending user data found for email {Email}", email);
                    return false;
                }

                // Check if user already exists (in case of duplicate verification attempts)
                if (await _userRepository.EmailExistsAsync(email))
                {
                    _logger.LogWarning("Email verification failed: User already exists for email {Email}", email);
                    // Clean up pending data
                    _verificationService.RemoveVerificationToken(token);
                    _pendingUserService.RemovePendingUser(email);
                    return false;
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(pendingUserData.Password);

                // Create the actual user account
                var customerRole = await _userRepository.GetRoleByNameAsync("Users");
                if (customerRole == null)
                {
                    _logger.LogError("Users role not found in the database.");
                    return false;
                }

                var newUser = new Users
                {
                    FullName = pendingUserData.FullName,
                    UserName = pendingUserData.UserName,
                    Email = pendingUserData.Email,
                    PasswordHash = passwordHash,
                    Phone = pendingUserData.PhoneNumber,
                    Address = pendingUserData.Address,
                    RoleId = customerRole.Id,
                    //IsCustomer = true,
                    //IsSupplier = false
                };

                // Save user to database
                var createdUser = await _userRepository.CreateUserAsync(newUser);
                if (createdUser == null)
                {
                    _logger.LogError("Failed to create user account for email: {Email}", email);
                    return false;
                }

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(email, pendingUserData.FullName);

                // Clean up verification data
                _verificationService.RemoveVerificationToken(token);
                _pendingUserService.RemovePendingUser(email);

                _logger.LogInformation("Email verification successful for user {UserName} ({Email})", 
                    createdUser.UserName, createdUser.Email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification for token: {TokenPrefix}...", token[..Math.Min(8, token.Length)]);
                return false;
            }
        }

        public async Task<bool> ResendVerificationEmailAsync(string email)
        {
            _logger.LogInformation("Resending verification email for: {Email}", email);

            try
            {
                // Check if user already exists
                if (await _userRepository.EmailExistsAsync(email))
                {
                    _logger.LogWarning("Resend verification failed: User already exists for email {Email}", email);
                    return false;
                }

                // Get pending user data
                var pendingUserData = _pendingUserService.GetPendingUser(email);
                if (pendingUserData == null)
                {
                    _logger.LogWarning("Resend verification failed: No pending user data found for email {Email}", email);
                    return false;
                }

                // Generate new verification token
                var verificationToken = _verificationService.GenerateVerificationToken(email);

                // Send verification email
                await _emailService.SendEmailVerificationAsync(email, pendingUserData.FullName, verificationToken);

                _logger.LogInformation("Verification email resent successfully for email: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification email for: {Email}", email);
                return false;
            }
        }

        public async Task SendPasswordResetLinkAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user != null)
            {
                var token = _passwordResetService.GeneratePasswordResetToken(email);
                await _emailService.SendPasswordResetAsync(email, user.FullName, token);
            }
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            if (_passwordResetService.ValidatePasswordResetToken(token, out var email))
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user != null)
                {
                    // Check if the new password is the same as the old one
                    if (BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash))
                    {
                        throw new InvalidOperationException("New password cannot be the same as the old password.");
                    }
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    await _userRepository.UpdateAsync(user);
                    _passwordResetService.RemovePasswordResetToken(token);
                    return true;
                }
            }
            return false;
        }
    }
}