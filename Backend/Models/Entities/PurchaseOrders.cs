using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class PurchaseOrders : BaseEntity
    {
        [Required]
        public string PurchaseOrderNumber { get; set; } = null!;

        [Required]
        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public string Status { get; set; } = "pending";
        [Required]
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Suppliers? Supplier { get; set; }
        public virtual ICollection<PurchaseOrderDetails> PurchaseOrderDetails { get; set; } = new HashSet<PurchaseOrderDetails>();
    }
}
