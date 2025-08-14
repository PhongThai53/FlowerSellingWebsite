using FlowerSellingWebsite.Models.Entities;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface IUserRepository
    {
        // User retrieval methods
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByPublicIdAsync(Guid publicId);
        Task<IEnumerable<User>> GetUsersAsync(int page, int pageSize, string? search, string? role);

        // Existence checks
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);

        // User management
        Task<User> CreateUserAsync(User user);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task SaveChangesAsync();

        // Password management
        Task<bool> VerifyPasswordAsync(User user, string password);
        Task<bool> UpdatePasswordAsync(User user, string newPasswordHash);

        // Role management
        Task<Role?> GetRoleByNameAsync(string roleName);
    }
}