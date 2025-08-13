using FlowerSellingWebsite.Models.Entities;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail);
        Task<bool> IsUsernameExistsAsync(string username);
        Task<bool> IsEmailExistsAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByPublicIdAsync(Guid publicId);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> VerifyPasswordAsync(User user, string password);
        Task<bool> UpdatePasswordAsync(User user, string newPasswordHash);
        Task<Role?> GetRoleByNameAsync(string roleName);
    }
}

