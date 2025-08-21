using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerImages : BaseEntity
    {
        [Required]
        [ForeignKey("Flower")]
        public int FlowerId { get; set; }

        [Required]
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(50)]
        public string? ImageType { get; set; }

        [Required]
        public bool IsPrimary { get; set; }
        [Required]
        public int DisplayOrder { get; set; }
        [Required]
        public bool IsActive { get; set; }

        // Navigation properties
        public virtual Flowers Flower { get; set; } = null!;
    }

}
