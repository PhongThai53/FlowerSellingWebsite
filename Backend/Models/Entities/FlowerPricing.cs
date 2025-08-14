using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerPricing : BaseEntity
    {
        [ForeignKey("FlowerCategory")]
        public int? FlowerCategoryId { get; set; }  // NULL = applies to all categories

        [ForeignKey("FlowerType")]
        public int? FlowerTypeId { get; set; }      // NULL = applies to all types

        [ForeignKey("FlowerColor")]
        public int? FlowerColorId { get; set; }     // NULL = applies to all colors

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
        public int Priority { get; set; }            // priority level (higher = more priority)
        [Required]
        public bool IsActive { get; set; }

        // Navigation properties
        public virtual FlowerCategories? FlowerCategory { get; set; }
        public virtual FlowerTypes? FlowerType { get; set; }
        public virtual FlowerColors? FlowerColor { get; set; }
        public virtual ICollection<FlowerPriceHistory> FlowerPriceHistories { get; set; } = new HashSet<FlowerPriceHistory>();
    }
}
