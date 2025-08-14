using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Payments : BaseEntity
    {
        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [Required]
        [ForeignKey("PaymentMethod")]
        public int PaymentMethodId { get; set; }

        [Required]
        public decimal Amount { get; set; }
        [Required]
        public DateTime PaymentDate { get; set; }
        [Required]
        public string Status { get; set; } = null!;

        // Navigation properties
        public virtual Orders Order { get; set; } = null!;
        public virtual PaymentMethods PaymentMethod { get; set; } = null!;
    }

}
