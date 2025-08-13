using FlowerSellingWebsite.Models.DTOs;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthenticationResponseDTO> LoginAsync(LoginRequestDTO loginRequest);
        Task<AuthenticationResponseDTO> RegisterAsync(RegisterRequestDTO registerRequest);
        Task<AuthenticationResponseDTO> ChangePasswordAsync(Guid userPublicId, ChangePasswordDTO changePasswordRequest);
        Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordRequest);
        Task<AuthenticationResponseDTO> ResetPasswordAsync(ResetPasswordDTO resetPasswordRequest);
        Task<UserDTO?> GetUserProfileAsync(Guid userPublicId);
        Task<bool> ValidateTokenAsync(string token);
    }
}

