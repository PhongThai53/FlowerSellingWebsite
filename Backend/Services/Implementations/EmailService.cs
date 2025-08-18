using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailVerificationAsync(string email, string fullName, string verificationToken)
        {
            var subject = "Verify Your Email Address - Floda Flower Shop";
            var verificationUrl = $"https://localhost:7062/api/auth/verify-email?token={verificationToken}";
            
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to Floda Flower Shop, {fullName}!</h2>
                    <p>Thank you for registering with us. To complete your registration, please verify your email address by clicking the link below:</p>
                    <p><a href='{verificationUrl}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Verify Email Address</a></p>
                    <p>Or copy and paste this link into your browser:</p>
                    <p>{verificationUrl}</p>
                    <p>This verification link will expire in 24 hours.</p>
                    <p>If you didn't create an account with us, please ignore this email.</p>
                    <br>
                    <p>Best regards,<br>Floda Flower Shop Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetAsync(string email, string fullName, string resetToken)
        {
            var subject = "Password Reset Request - Floda Flower Shop";
            var resetUrl = $"{_configuration["FrontendUrl"]}/html/auth/reset-password.html?token={resetToken}";
            
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>Hello {fullName},</p>
                    <p>We received a request to reset your password. Click the link below to reset your password:</p>
                    <p><a href='{resetUrl}' style='background-color: #f44336; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                    <p>Or copy and paste this link into your browser:</p>
                    <p>{resetUrl}</p>
                    <p>This reset link will expire in 1 hour.</p>
                    <p>If you didn't request a password reset, please ignore this email.</p>
                    <br>
                    <p>Best regards,<br>Floda Flower Shop Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string email, string fullName)
        {
            var subject = "Welcome to Floda Flower Shop!";
            
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to Floda Flower Shop, {fullName}!</h2>
                    <p>Your email has been successfully verified and your account is now active.</p>
                    <p>You can now enjoy all our services:</p>
                    <ul>
                        <li>Browse our beautiful flower collections</li>
                        <li>Place orders and track deliveries</li>
                        <li>Manage your account preferences</li>
                    </ul>
                    <p>Thank you for choosing Floda Flower Shop!</p>
                    <br>
                    <p>Best regards,<br>Floda Flower Shop Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
                var smtpUser = _configuration["EmailSettings:Username"];
                var smtpPass = _configuration["EmailSettings:Password"];
                var fromEmail = _configuration["EmailSettings:SenderEmail"];
                var fromName = _configuration["EmailSettings:SenderName"];

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? smtpUser ?? "", fromName ?? "Floda Flower Shop"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }
    }
}
