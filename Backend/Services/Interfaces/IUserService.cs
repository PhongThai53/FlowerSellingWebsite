using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.Entities;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<UserDTO> RegisterAsync(RegisterRequestDTO request);
        Task<UserDTO> GetUserByIdAsync(Guid publicId);
        Task<UserDTO> GetCurrentUserAsync();
        Task<UserDTO> CreateUserAsync(CreateUserRequestDTO request);
        Task<UserDTO> UpdateUserAsync(Guid publicId, UpdateUserRequestDTO request);
        Task<bool> DeleteUserAsync(Guid publicId);
        Task<bool> ChangePasswordAsync(Guid publicId, ChangePasswordRequestDTO request);

        Task<PagedResult<UserDTO>> GetUsersAsync(UrlQueryParams urlQueryParams);
        Task<IEnumerable<UserDTO>> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null, string? role = null);
        Task<bool> ActivateUserAsync(Guid publicId);
        Task<bool> DeactivateUserAsync(Guid publicId);
        Task<IEnumerable<RoleDTO>> GetRolesAsync();
        
        // Email verification methods
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> ResendVerificationEmailAsync(string email);
        Task SendPasswordResetLinkAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        
        // Email existence check
        Task<bool> EmailExistsAsync(string email);
    }
}
