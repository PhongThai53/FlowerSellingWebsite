using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace FlowerSellingWebsite.Services.Implementations
{
    public class ContactService : IContactService
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public ContactService(IEmailService emailService, IConfiguration configuration)
        {
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task SendContactEmailAsync(ContactFormDTO contactForm)
        {
            var adminEmail = _configuration["EmailSettings:AdminEmail"];
            if (string.IsNullOrEmpty(adminEmail))
            {
                // Handle missing admin email configuration
                throw new System.Exception("Admin email is not configured.");
            }

            var subject = $"New Contact Form Submission: {contactForm.Subject}";
            var body = $@"
                <html>
                <body>
                    <h2>New Contact Form Submission</h2>
                    <p><strong>Name:</strong> {contactForm.Name}</p>
                    <p><strong>Email:</strong> {contactForm.Email}</p>
                    <p><strong>Phone:</strong> {contactForm.Phone}</p>
                    <hr>
                    <p><strong>Message:</strong></p>
                    <p>{contactForm.Message}</p>
                </body>
                </html>";

            await _emailService.SendEmailAsync(adminEmail, subject, body);
        }
    }
}
