using System.ComponentModel.DataAnnotations;
using FlowerSellingWebsite.Models.Validation;

namespace FlowerSellingWebsite.Models.DTOs
{
    public class UpdateAccountRequestDTO
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [NoWhitespace(ErrorMessage = "Full name cannot contain only spaces")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
        [NoWhitespace(ErrorMessage = "Username cannot contain only spaces")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [RegularExpression(@"^[\d\s\-\(\)\+]+$", ErrorMessage = "Phone number can only contain digits, spaces, hyphens, parentheses, and plus signs")]
        [NoWhitespace(ErrorMessage = "Phone number cannot contain only spaces")]
        [ValidPhoneNumber(ErrorMessage = "Please enter a valid phone number (10-15 digits)")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [NoWhitespace(ErrorMessage = "Address cannot contain only spaces")]
        public string? Address { get; set; }
    }
}


