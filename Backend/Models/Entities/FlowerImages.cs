using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerImages : BaseEntity
    {
        [Required]
        [ForeignKey("Flower")]
        public int FlowerId { get; set; }  // Only connect to Flowers

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string ImageType { get; set; } = null!; // thumbnail, banner, icon, gallery

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
        public virtual Flowers Flower { get; set; } = null!;
    }

}
