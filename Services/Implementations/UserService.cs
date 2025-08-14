using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Services.Interfaces;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class UserService : IUserService
    {
        public Task<bool> ActivateUserAsync(Guid publicId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ChangePasswordAsync(Guid publicId, ChangePasswordRequestDTO request)
        {
            throw new NotImplementedException();
        }

        public Task<UserDTO> CreateUserAsync(CreateUserRequestDTO request)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeactivateUserAsync(Guid publicId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteUserAsync(Guid publicId)
        {
            throw new NotImplementedException();
        }

        public Task<UserDTO> GetCurrentUserAsync()
        {
            throw new NotImplementedException();
        }

        public Task<UserDTO> GetUserByIdAsync(Guid publicId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserDTO>> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null, string? role = null)
        {
            throw new NotImplementedException();
        }

        public Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            throw new NotImplementedException();
        }

        public Task<UserDTO> RegisterAsync(RegisterRequestDTO request)
        {
            throw new NotImplementedException();
        }

        public Task<UserDTO> UpdateUserAsync(Guid publicId, UpdateUserRequestDTO request)
        {
            throw new NotImplementedException();
        }
    }
}
