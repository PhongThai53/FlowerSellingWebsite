using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerPriceHistory : BaseEntity
    {
        [Required]
        [ForeignKey("FlowerPricing")]
        public int FlowerPricingId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal OldPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal NewPrice { get; set; }

        public string? ChangeReason { get; set; }

        [Required]
        [ForeignKey("ChangedByUser")]
        public int ChangedByUserId { get; set; }

        [Required]
        public DateTime ChangeDate { get; set; }

        // Navigation properties
        public virtual FlowerPricing? FlowerPricing { get; set; }
        public virtual Users? ChangedByUser { get; set; }
    }
}
