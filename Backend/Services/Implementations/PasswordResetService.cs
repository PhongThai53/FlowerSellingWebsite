using FlowerSellingWebsite.Services.Interfaces;
using System.Collections.Concurrent;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly ConcurrentDictionary<string, (string Email, DateTime Expiry)> _resetTokens = new();
        private const int TokenExpiryHours = 1;

        public string GeneratePasswordResetToken(string email)
        {
            // Invalidate any existing tokens for this email
            var existingToken = _resetTokens.FirstOrDefault(x => x.Value.Email == email).Key;
            if (existingToken != null)
            {
                _resetTokens.TryRemove(existingToken, out _);
            }

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            _resetTokens[token] = (email, DateTime.UtcNow.AddHours(TokenExpiryHours));
            return token;
        }

        public bool ValidatePasswordResetToken(string token, out string email)
        {
            email = null;
            if (_resetTokens.TryGetValue(token, out var tokenData) && tokenData.Expiry > DateTime.UtcNow)
            {
                email = tokenData.Email;
                return true;
            }
            return false;
        }

        public void RemovePasswordResetToken(string token)
        {
            _resetTokens.TryRemove(token, out _);
        }

        public void CleanupExpiredTokens()
        {
            var expiredTokens = _resetTokens.Where(x => x.Value.Expiry <= DateTime.UtcNow).ToList();
            foreach (var expiredToken in expiredTokens)
            {
                _resetTokens.TryRemove(expiredToken.Key, out _);
            }
        }
    }
}
