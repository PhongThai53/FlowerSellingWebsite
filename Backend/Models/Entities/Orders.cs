using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Orders : BaseEntity
    {
        [Required]
        public string OrderNumber { get; set; } = null!;

        [Required]
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public string Status { get; set; } = null!;
        [Required]
        public decimal Subtotal { get; set; }
        [Required]
        public decimal TaxAmount { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Users Customer { get; set; } = null!;
        public virtual ICollection<OrderDetails> OrderDetails { get; set; } = new HashSet<OrderDetails>();
        public virtual ICollection<Payments> Payments { get; set; } = new HashSet<Payments>();
        public virtual ICollection<Deliveries> Deliveries { get; set; } = new HashSet<Deliveries>();
    }
}
