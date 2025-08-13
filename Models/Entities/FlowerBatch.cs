using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    [Table("FlowerBatches")]
    public class FlowerBatch : BaseEntity
    {
        [Required]
        public int FlowerId { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [StringLength(50)]
        public string? BatchCode { get; set; }

        [Required]
        public DateTime ImportDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be positive")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "QuantityAvailable must be positive")]
        public int QuantityAvailable { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "UnitPrice must be positive")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "TotalAmount must be positive")]
        public decimal TotalAmount { get; set; }

        // Navigation Properties
        [ForeignKey("FlowerId")]
        public virtual Flower Flower { get; set; } = null!;

        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; } = null!;

        public virtual ICollection<FlowerDamageLog> FlowerDamageLogs { get; set; } = new List<FlowerDamageLog>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
