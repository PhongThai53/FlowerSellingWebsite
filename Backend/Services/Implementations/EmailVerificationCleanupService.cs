using FlowerSellingWebsite.Services.Interfaces;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class EmailVerificationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailVerificationCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1); // Run cleanup every hour

        public EmailVerificationCleanupService(
            IServiceProvider serviceProvider, 
            ILogger<EmailVerificationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting email verification cleanup task");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var verificationService = scope.ServiceProvider.GetRequiredService<IEmailVerificationService>();
                        var pendingUserService = scope.ServiceProvider.GetRequiredService<IPendingUserService>();

                        // Clean up expired verification tokens
                        verificationService.CleanupExpiredTokens();

                        // Clean up expired pending users
                        pendingUserService.CleanupExpiredPendingUsers();
                    }

                    _logger.LogInformation("Email verification cleanup task completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during email verification cleanup");
                }

                // Wait for the next cleanup interval
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }
}
