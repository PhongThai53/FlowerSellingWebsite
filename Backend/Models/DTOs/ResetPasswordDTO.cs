using System.ComponentModel.DataAnnotations;
using FlowerSellingWebsite.Models.Validation;

namespace FlowerSellingWebsite.Models.DTOs
{
    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "Reset token is required")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@$!%*?&]{6,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number")]
        [NoWhitespace(ErrorMessage = "Password cannot contain leading or trailing spaces")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation password do not match")]
        [NoWhitespace(ErrorMessage = "Password confirmation cannot contain leading or trailing spaces")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

