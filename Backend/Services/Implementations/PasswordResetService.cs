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
        private readonly ILogger<PasswordResetService> _logger;
        private const int TokenExpiryHours = 1;

        public PasswordResetService(ILogger<PasswordResetService> logger)
        {
            _logger = logger;
            _logger.LogInformation("PasswordResetService initialized - all previous tokens are now invalid");
        }

        public string GeneratePasswordResetToken(string email)
        {
            _logger.LogInformation("Generating password reset token for email: {Email}", email);
            
            // Invalidate any existing tokens for this email
            var existingToken = _resetTokens.FirstOrDefault(x => x.Value.Email == email).Key;
            if (existingToken != null)
            {
                _logger.LogInformation("Removing existing token for email: {Email}", email);
                _resetTokens.TryRemove(existingToken, out _);
            }

            // Use URL-safe Base64 encoding to avoid issues with special characters in URLs
            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(tokenBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
                
            var expiry = DateTime.UtcNow.AddHours(TokenExpiryHours);
            _resetTokens[token] = (email, expiry);
            
            _logger.LogInformation("Generated token: {Token} for email: {Email}, expires at: {Expiry}", 
                token, email, expiry);
            
            return token;
        }

        public bool ValidatePasswordResetToken(string token, out string email)
        {
            email = null;
            _logger.LogInformation("Validating password reset token: {Token}", token);
            _logger.LogInformation("Current active tokens count: {Count}", _resetTokens.Count);
            
            if (_resetTokens.TryGetValue(token, out var tokenData))
            {
                _logger.LogInformation("Token found in dictionary. Expiry: {Expiry}, Current time: {CurrentTime}", 
                    tokenData.Expiry, DateTime.UtcNow);
                
                if (tokenData.Expiry > DateTime.UtcNow)
                {
                    email = tokenData.Email;
                    _logger.LogInformation("Token validation successful for email: {Email}", email);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Token has expired. Expiry: {Expiry}, Current: {CurrentTime}", 
                        tokenData.Expiry, DateTime.UtcNow);
                }
            }
            else
            {
                _logger.LogWarning("Token not found in dictionary: {Token}", token);
                // Log all current tokens for debugging (be careful in production)
                foreach (var kvp in _resetTokens)
                {
                    _logger.LogDebug("Available token: {AvailableToken} for email: {Email}", 
                        kvp.Key, kvp.Value.Email);
                }
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
