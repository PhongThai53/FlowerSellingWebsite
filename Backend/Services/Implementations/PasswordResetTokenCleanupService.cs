using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class PasswordResetTokenCleanupService : BackgroundService
    {
        private readonly ILogger<PasswordResetTokenCleanupService> _logger;
        private readonly IPasswordResetService _passwordResetService;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

        public PasswordResetTokenCleanupService(ILogger<PasswordResetTokenCleanupService> logger, IPasswordResetService passwordResetService)
        {
            _logger = logger;
            _passwordResetService = passwordResetService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Password reset token cleanup service is running.");
                
                _passwordResetService.CleanupExpiredTokens();

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }
}
