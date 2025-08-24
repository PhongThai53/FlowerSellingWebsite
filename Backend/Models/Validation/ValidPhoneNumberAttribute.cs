using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FlowerSellingWebsite.Models.Validation
{
    public class ValidPhoneNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var phoneNumber = value.ToString();
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return ValidationResult.Success;

            // Remove all non-digit characters for validation
            var cleanPhone = Regex.Replace(phoneNumber, @"[\s\-\(\)\+]", "");
            
            // Check if it's a valid phone number (10-15 digits)
            if (!Regex.IsMatch(cleanPhone, @"^[0-9]{10,15}$"))
            {
                return new ValidationResult($"{validationContext.DisplayName} must be a valid phone number with 10-15 digits.");
            }

            return ValidationResult.Success;
        }
    }
}
