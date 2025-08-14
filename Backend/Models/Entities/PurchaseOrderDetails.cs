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
        [ForeignKey("Flower")]
        public int FlowerId { get; set; }

        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal UnitPrice { get; set; }
        [Required]
        public decimal LineTotal { get; set; }

        // Navigation properties
        public virtual PurchaseOrders PurchaseOrder { get; set; } = null!;
        public virtual Flowers Flower { get; set; } = null!;
    }
}
