using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerDamageLogs : BaseEntity
    {
        [Required]
        public int PurchaseOrderDetailId { get; set; }

        [Required]
        public int ReportedByUserId { get; set; }

        [Required]
        public int DamagedQuantity { get; set; }
        [Required]
        public string DamageReason { get; set; } = null!;
        [Required]
        public DateTime DamageDate { get; set; }
        public string? Notes { get; set; }

        [ForeignKey(nameof(PurchaseOrderDetailId))]
        public virtual PurchaseOrderDetails PurchaseOrderDetail { get; set; } = null!;

        [ForeignKey(nameof(ReportedByUserId))]
        public virtual Users ReportedByUser { get; set; } = null!;
    }
}
