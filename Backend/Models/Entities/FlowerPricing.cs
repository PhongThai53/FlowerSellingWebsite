using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerPricing : BaseEntity
    {
        [Required]
        [ForeignKey("Flower")]
        public int FlowerId { get; set; }  // Only connect to Flowers

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = null!; // VND, USD...

        [Required]
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }    // NULL = no expiration

        [Required]
        [StringLength(20)]
        public string PriceType { get; set; } = null!; // retail, wholesale, promotional

        [Required]
        public bool IsActive { get; set; }

        // Navigation properties
        public virtual Flowers Flower { get; set; } = null!;
        public virtual ICollection<FlowerPriceHistory> FlowerPriceHistories { get; set; } = new HashSet<FlowerPriceHistory>();
    }
}
