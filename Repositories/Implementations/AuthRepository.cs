using FlowerSelling.Data;
using FlowerSellingWebsite.Models.Entities;
using FlowerSellingWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace FlowerSellingWebsite.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly FlowerSellingDbContext _context;

        public AuthRepository(FlowerSellingDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserName == usernameOrEmail || u.Email == usernameOrEmail);
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.UserName == username);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(User user)
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

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserByPublicIdAsync(Guid publicId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.PublicId == publicId);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> VerifyPasswordAsync(User user, string password)
        {
            return await Task.FromResult(
                !string.IsNullOrEmpty(user.PasswordHash) && 
                BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)
            );
        }

        public async Task<bool> UpdatePasswordAsync(User user, string newPasswordHash)
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

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
        }
    }
}

