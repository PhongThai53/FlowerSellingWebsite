using FlowerSellingWebsite.Models.DTOs;
using FlowerSellingWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FlowerSellingWebsite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IContactService contactService, ILogger<ContactController> logger)
        {
            _contactService = contactService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitContactForm([FromBody] ContactFormDTO contactForm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                var errorHtml = "<div class='alert alert-danger'><ul>";
                foreach (var error in errors)
                {
                    errorHtml += $"<li>{error}</li>";
                }
                errorHtml += "</ul></div>";
                return new ContentResult { Content = errorHtml, ContentType = "text/html", StatusCode = 400 };
            }

            try
            {
                await _contactService.SendContactEmailAsync(contactForm);
                var successHtml = "<div class='alert alert-success'>Thank you for your message! We will get back to you shortly.</div>";
                return Content(successHtml, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the contact form.");
                var errorHtml = "<div class='alert alert-danger'>An unexpected error occurred. Please try again later.</div>";
                return new ContentResult { Content = errorHtml, ContentType = "text/html", StatusCode = 500 };
            }
        }
    }
}
