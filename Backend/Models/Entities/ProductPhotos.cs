using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class ProductPhotos : BaseEntity
    {
        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required]
        public string Url { get; set; } = null!;
        [Required]
        public bool IsPrimary { get; set; }

        // Navigation properties
        public virtual Products Product { get; set; } = null!;
    }
}
