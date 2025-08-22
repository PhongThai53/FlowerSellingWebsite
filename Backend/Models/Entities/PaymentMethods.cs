using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class PaymentMethods : BaseEntity
    {
        [Required]
        public string MethodName { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public string MethodType { get; set; } = "Standard"; // Standard, VNPay, etc.

        public string? IconClass { get; set; } // CSS class for payment method icon
        public string? DisplayName { get; set; } // User-friendly display name

        // Navigation properties
        public virtual ICollection<Payments> Payments { get; set; } = new HashSet<Payments>();
    }
}
