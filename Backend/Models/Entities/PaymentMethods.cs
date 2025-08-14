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

        // Navigation properties
        public virtual ICollection<Payments> Payments { get; set; } = new HashSet<Payments>();
    }
}
