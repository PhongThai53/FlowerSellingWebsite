using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Products : BaseEntity
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Category { get; set; }

        // Navigation properties
        public virtual ICollection<OrderDetails> OrderDetails { get; set; } = new HashSet<OrderDetails>();
        public virtual ICollection<ProductPhotos> ProductPhotos { get; set; } = new HashSet<ProductPhotos>();
        public virtual ICollection<ProductFlowers> ProductFlowers { get; set; } = new HashSet<ProductFlowers>();
    }
}
