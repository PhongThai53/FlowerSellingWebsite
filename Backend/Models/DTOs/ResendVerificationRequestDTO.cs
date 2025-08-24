using System.ComponentModel.DataAnnotations;
using FlowerSellingWebsite.Models.Validation;

namespace FlowerSellingWebsite.Models.DTOs
{
    public class ResendVerificationRequestDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Invalid email format")]
        [NoWhitespace]
        public string Email { get; set; } = string.Empty;
    }
}
