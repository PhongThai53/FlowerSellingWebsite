using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using FlowerSellingWebsite.Services.Interfaces;
using ProjectGreenLens.Services.Interfaces;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<UserService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(
            IUserRepository userRepository,
            IJwtService jwtService,
            ILogger<UserService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
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
                    _logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
                    throw new UnauthorizedAccessException("Invalid email or password");
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

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Create new user - assuming Customer role by default
                var newUser = new User
                {
                    FullName = request.FullName,
                    UserName = request.UserName,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Phone = request.PhoneNumber,
                    Address = request.Address,
                    RoleId = 4, // Customer role ID from your seeded data
                    IsCustomer = true,
                    IsSupplier = false
                };

                // Save user to database using the merged repository
                var createdUser = await _userRepository.CreateUserAsync(newUser);
                if (createdUser == null || createdUser.Role == null)
                {
                    _logger.LogError("Failed to create user or load role for user: {UserName}", request.UserName);
                    throw new InvalidOperationException("Failed to create user account");
                }

                _logger.LogInformation("Registration successful for user {UserName} ({Email}) with PublicId: {PublicId}", 
                    createdUser.UserName, createdUser.Email, createdUser.PublicId);

                return MapToUserDTO(createdUser);
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
                    throw new KeyNotFoundException($"User with ID {publicId} not found");
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

                // Create new user
                var newUser = new User
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Phone = request.PhoneNumber,
                    Address = request.Address,
                    RoleId = GetRoleIdByName(request.RoleName),
                    IsCustomer = request.RoleName.Equals("Customer", StringComparison.OrdinalIgnoreCase),
                    IsSupplier = request.RoleName.Equals("Supplier", StringComparison.OrdinalIgnoreCase)
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
                    throw new KeyNotFoundException($"User with ID {publicId} not found");
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
                    throw new KeyNotFoundException($"User with ID {publicId} not found");
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

        private static UserDTO MapToUserDTO(User user)
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

        private static int GetRoleIdByName(string roleName)
        {
            return roleName.ToLower() switch
            {
                "admin" => 1,
                "manager" => 2,
                "staff" => 3,
                "customer" => 4,
                "supplier" => 5,
                _ => 4 // Default to Customer
            };
        }
    }
}