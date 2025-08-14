using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerCategoryImages : BaseEntity
    {
        [ForeignKey("FlowerCategory")]
        public int? FlowerCategoryId { get; set; }  // NULL = applies to all categories

        [ForeignKey("FlowerType")]
        public int? FlowerTypeId { get; set; }      // NULL = applies to all types

        [ForeignKey("FlowerColor")]
        public int? FlowerColorId { get; set; }     // NULL = applies to all colors

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string ImageType { get; set; } = null!; // thumbnail, banner, icon, gallery

        [Required]
        public int Priority { get; set; }            // priority level (higher = more priority)
        [Required]
        public bool IsPrimary { get; set; }
        [Required]
        public int DisplayOrder { get; set; }
        [Required]
        public DateTime EffectiveDate { get; set; }  // from date
        public DateTime? ExpiryDate { get; set; }    // to date (NULL = permanent)
        [Required]
        public bool IsActive { get; set; }

        // Navigation properties
        public virtual FlowerCategories? FlowerCategory { get; set; }
        public virtual FlowerTypes? FlowerType { get; set; }
        public virtual FlowerColors? FlowerColor { get; set; }
    }
}
