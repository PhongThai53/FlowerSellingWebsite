using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Orders : BaseEntity
    {
        [Required]
        public string OrderNumber { get; set; } = null!;

        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [Required]
        [ForeignKey("CreatedByUser")]
        public int CreatedByUserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }

        [Required]
        public string Status { get; set; } = null!;
        public bool IsSaleOrder { get; set; }

        [Required]
        public decimal Subtotal { get; set; }
        [Required]
        public decimal TaxAmount { get; set; }
        [Required]
        public decimal EstimatedTotalAmount { get; set; }
        public decimal? FinalTotalAmount { get; set; }

        public string? Notes { get; set; }
        public string? SupplierNotes { get; set; }

        // Navigation properties
        public virtual Users Customer { get; set; } = null!;
        public virtual Suppliers? Supplier { get; set; }
        public virtual Users CreatedByUser { get; set; } = null!;
        public virtual ICollection<OrderDetails> OrderDetails { get; set; } = new HashSet<OrderDetails>();
        public virtual ICollection<Payments> Payments { get; set; } = new HashSet<Payments>();
        public virtual ICollection<Deliveries> Deliveries { get; set; } = new HashSet<Deliveries>();
    }
}
