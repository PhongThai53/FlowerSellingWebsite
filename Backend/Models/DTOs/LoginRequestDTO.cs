using System.ComponentModel.DataAnnotations;
using FlowerSellingWebsite.Models.Validation;

namespace FlowerSellingWebsite.Models.DTOs
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Invalid email format")]
        [NoWhitespace]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        [NoWhitespace]
        public string Password { get; set; } = string.Empty;
    }
}
