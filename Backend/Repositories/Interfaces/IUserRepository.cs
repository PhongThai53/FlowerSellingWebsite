using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Models.Entities;

namespace FlowerSellingWebsite.Repositories.Interfaces
{
    public interface IUserRepository
    {
        // Users retrieval methods
        Task<Users?> GetByEmailAsync(string email);
        Task<Users?> GetByUsernameAsync(string username);
        Task<Users?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<Users?> GetByIdAsync(int id);
        Task<Users?> GetByPublicIdAsync(Guid publicId);
        Task<IEnumerable<Users>> GetUsersAsync(int page, int pageSize, string? search, string? role);

        // Existence checks
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);

        // Users management
        Task<PagedResult<Users>> GetUsersAsync(UrlQueryParams urlQueryParams);
        Task<Users> CreateUserAsync(Users user);
        Task AddAsync(Users user);
        Task UpdateAsync(Users user);
        Task SaveChangesAsync();

        // Password management
        Task<bool> VerifyPasswordAsync(Users user, string password);
        Task<bool> UpdatePasswordAsync(Users user, string newPasswordHash);

        // Role management
        Task<Roles?> GetRoleByNameAsync(string roleName);

        // Email verification (using existing fields creatively)
        Task<Users?> GetUserByEmailForVerificationAsync(string email);
    }
}