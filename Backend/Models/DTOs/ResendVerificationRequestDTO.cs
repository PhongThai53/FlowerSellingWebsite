using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs
{
    public class ResendVerificationRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
