using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Services.Interfaces;
using System.Collections.Concurrent;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class PendingUserService : IPendingUserService
    {
        private readonly ConcurrentDictionary<string, PendingUserData> _pendingUsers;
        private readonly ILogger<PendingUserService> _logger;

        public PendingUserService(ILogger<PendingUserService> logger)
        {
            _pendingUsers = new ConcurrentDictionary<string, PendingUserData>();
            _logger = logger;
        }

        public void StorePendingUser(string email, RegisterRequestDTO userData)
        {
            var pendingData = new PendingUserData
            {
                UserData = userData,
                CreatedAt = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddHours(24) // 24 hours to complete verification
            };

            _pendingUsers[email.ToLowerInvariant()] = pendingData;
            _logger.LogInformation("Stored pending user data for email: {Email}", email);
        }

        public RegisterRequestDTO? GetPendingUser(string email)
        {
            if (_pendingUsers.TryGetValue(email.ToLowerInvariant(), out var pendingData))
            {
                if (DateTime.UtcNow <= pendingData.ExpiryTime)
                {
                    return pendingData.UserData;
                }
                else
                {
                    // Remove expired data
                    _pendingUsers.TryRemove(email.ToLowerInvariant(), out _);
                    _logger.LogWarning("Pending user data expired for email: {Email}", email);
                }
            }

            return null;
        }

        public void RemovePendingUser(string email)
        {
            if (_pendingUsers.TryRemove(email.ToLowerInvariant(), out _))
            {
                _logger.LogInformation("Removed pending user data for email: {Email}", email);
            }
        }

        public void CleanupExpiredPendingUsers()
        {
            var expiredEmails = _pendingUsers
                .Where(kvp => DateTime.UtcNow > kvp.Value.ExpiryTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var email in expiredEmails)
            {
                _pendingUsers.TryRemove(email, out _);
            }

            if (expiredEmails.Any())
            {
                _logger.LogInformation("Cleaned up {Count} expired pending users", expiredEmails.Count);
            }
        }

        private class PendingUserData
        {
            public RegisterRequestDTO UserData { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
            public DateTime ExpiryTime { get; set; }
        }
    }
}
