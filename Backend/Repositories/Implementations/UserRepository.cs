using FlowerSelling.Data.FlowerSellingWebsite.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly FlowerSellingDbContext _context;

        public UserRepository(FlowerSellingDbContext context)
        {
            _context = context;
        }

        // Users retrieval methods
        public async Task<Users?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<Users?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.UserName == username && !u.IsDeleted);
        }

        public async Task<Users?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await _context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u =>
                    (u.UserName == usernameOrEmail || u.Email == usernameOrEmail) &&
                    !u.IsDeleted);
        }

        public async Task<Users?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<Users?> GetByPublicIdAsync(Guid publicId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.PublicId == publicId && !u.IsDeleted);
        }

        public async Task<IEnumerable<Users>> GetUsersAsync(int page, int pageSize, string? search, string? role)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Where(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    (u.FullName != null && u.FullName.Contains(search)) ||
                    (u.UserName != null && u.UserName.Contains(search)) ||
                    (u.Email != null && u.Email.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role.RoleName == role);
            }

            return await query.OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Existence checks
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.UserName == username && !u.IsDeleted);
        }

        // Users management
        public async Task<Users> CreateUserAsync(Users user)
        {
            // Use a database transaction to ensure atomic operation
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Reload the role information after saving
                await _context.Entry(user).Reference(u => u.Role).LoadAsync();

                // Commit the transaction if everything succeeded
                await transaction.CommitAsync();

                return user;
            }
            catch
            {
                // Rollback the transaction if any error occurs
                await transaction.RollbackAsync();
                throw; // Re-throw the exception to be handled by the service layer
            }
        }

        public async Task AddAsync(Users user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Users user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // Password management
        public async Task<bool> VerifyPasswordAsync(Users user, string password)
        {
            return await Task.FromResult(
                !string.IsNullOrEmpty(user.PasswordHash) &&
                BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)
            );
        }

        public async Task<bool> UpdatePasswordAsync(Users user, string newPasswordHash)
        {
            try
            {
                user.PasswordHash = newPasswordHash;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Role management
        public async Task<Roles?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
        }

        // Email verification (using existing fields creatively)
        public async Task<User?> GetUserByEmailForVerificationAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}