using FlowerSellingWebsite.Models.DTOs;

namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IPendingUserService
    {
        void StorePendingUser(string email, RegisterRequestDTO userData);
        RegisterRequestDTO? GetPendingUser(string email);
        void RemovePendingUser(string email);
        void CleanupExpiredPendingUsers();
    }
}
