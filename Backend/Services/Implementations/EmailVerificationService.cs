using FlowerSellingWebsite.Services.Interfaces;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly ConcurrentDictionary<string, VerificationTokenData> _verificationTokens;
        private readonly ILogger<EmailVerificationService> _logger;

        public EmailVerificationService(ILogger<EmailVerificationService> logger)
        {
            _verificationTokens = new ConcurrentDictionary<string, VerificationTokenData>();
            _logger = logger;
        }

        public string GenerateVerificationToken(string email)
        {
            // Remove any existing token for this email
            var existingTokens = _verificationTokens
                .Where(kvp => kvp.Value.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var existingToken in existingTokens)
            {
                _verificationTokens.TryRemove(existingToken, out _);
            }

            // Generate new token
            var token = GenerateSecureToken();
            var tokenData = new VerificationTokenData
            {
                Email = email,
                ExpiryTime = DateTime.UtcNow.AddHours(24) // 24 hours expiry
            };

            _verificationTokens[token] = tokenData;
            _logger.LogInformation("Generated verification token for email: {Email}", email);

            return token;
        }

        public bool ValidateVerificationToken(string token, out string email)
        {
            email = string.Empty;

            if (!_verificationTokens.TryGetValue(token, out var tokenData))
            {
                return false;
            }

            if (DateTime.UtcNow > tokenData.ExpiryTime)
            {
                _verificationTokens.TryRemove(token, out _);
                _logger.LogWarning("Verification token expired for email: {Email}", tokenData.Email);
                return false;
            }

            email = tokenData.Email;
            return true;
        }

        public void RemoveVerificationToken(string token)
        {
            if (_verificationTokens.TryRemove(token, out var tokenData))
            {
                _logger.LogInformation("Removed verification token for email: {Email}", tokenData.Email);
            }
        }

        public bool IsEmailPendingVerification(string email)
        {
            return _verificationTokens.Values
                .Any(token => token.Email.Equals(email, StringComparison.OrdinalIgnoreCase) 
                           && DateTime.UtcNow <= token.ExpiryTime);
        }

        public void CleanupExpiredTokens()
        {
            var expiredTokens = _verificationTokens
                .Where(kvp => DateTime.UtcNow > kvp.Value.ExpiryTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var expiredToken in expiredTokens)
            {
                if (_verificationTokens.TryRemove(expiredToken, out var tokenData))
                {
                    _logger.LogInformation("Cleaned up expired verification token for email: {Email}", tokenData.Email);
                }
            }

            if (expiredTokens.Any())
            {
                _logger.LogInformation("Cleaned up {Count} expired verification tokens", expiredTokens.Count);
            }
        }

        private static string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenBytes = new byte[32];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        private class VerificationTokenData
        {
            public string Email { get; set; } = string.Empty;
            public DateTime ExpiryTime { get; set; }
        }
    }
}
