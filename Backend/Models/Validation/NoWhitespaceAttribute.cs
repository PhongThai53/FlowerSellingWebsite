using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Validation
{
    public class NoWhitespaceAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var stringValue = value.ToString();
            if (string.IsNullOrEmpty(stringValue))
                return ValidationResult.Success;

            // Check for leading/trailing spaces
            if (stringValue != stringValue.Trim())
            {
                return new ValidationResult($"{validationContext.DisplayName} cannot have leading or trailing spaces.");
            }

            // Check for whitespace-only strings
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return new ValidationResult($"{validationContext.DisplayName} cannot contain only spaces.");
            }

            return ValidationResult.Success;
        }
    }
}
