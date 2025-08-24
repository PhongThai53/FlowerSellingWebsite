using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class PurchaseOrderDetails : BaseEntity
    {
        [Required]
        [ForeignKey("PurchaseOrder")]
        public int PurchaseOrderId { get; set; }

        [Required]
        [ForeignKey("SupplierListing")]
        public int SupplierListingId { get; set; }   // 🔥 thay vì FlowerId

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public decimal LineTotal { get; set; }

        // Navigation properties
        public virtual PurchaseOrders? PurchaseOrder { get; set; }
        public virtual SupplierListings? SupplierListing { get; set; }   // 🔥 join vào listing
        public virtual ICollection<FlowerDamageLogs> FlowerDamageLogs { get; set; } = new HashSet<FlowerDamageLogs>();
    }
}
