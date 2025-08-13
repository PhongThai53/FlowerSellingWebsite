using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    [Table("ProductPhotos")]
    public class ProductPhoto : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(300)]
        public string PhotoUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
