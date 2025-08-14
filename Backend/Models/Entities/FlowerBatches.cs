using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerBatches : BaseEntity
    {
        [Required]
        [ForeignKey("Flower")]
        public int FlowerId { get; set; }

        [Required]
        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }

        [Required]
        public string BatchCode { get; set; } = null!;
        [Required]
        public DateTime ImportDate { get; set; }
        [Required]
        public DateTime ExpiryDate { get; set; }     // actual expiry date of the batch
        [Required]
        public int QuantityAvailable { get; set; }
        [Required]
        public decimal UnitPrice { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }

        // Navigation properties
        public virtual Flowers Flower { get; set; } = null!;
        public virtual Suppliers Supplier { get; set; } = null!;
        public virtual ICollection<FlowerDamageLogs> FlowerDamageLogs { get; set; } = new HashSet<FlowerDamageLogs>();
    }
}
