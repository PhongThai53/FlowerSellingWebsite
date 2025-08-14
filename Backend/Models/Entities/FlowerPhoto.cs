using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    [Table("FlowerPhotos")]
    public class FlowerPhoto : BaseEntity
    {
        [Required]
        public int FlowerId { get; set; }

        [Required]
        [StringLength(300)]
        public string PhotoUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;

        // Navigation Properties
        [ForeignKey("FlowerId")]
        public virtual Flower Flower { get; set; } = null!;
    }
}
