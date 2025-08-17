using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Products : BaseEntity
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string Url { get; set; } = null!;

        [Required]
        public int CategoryId { get; set; }

        // Navigation properties
        public virtual ICollection<OrderDetails> OrderDetails { get; set; } = new HashSet<OrderDetails>();
        public virtual ICollection<ProductPhotos> ProductPhotos { get; set; } = new HashSet<ProductPhotos>();
        public virtual ICollection<ProductFlowers> ProductFlowers { get; set; } = new HashSet<ProductFlowers>();
        public ICollection<ProductPriceHistories> PriceHistories { get; set; } = new List<ProductPriceHistories>();

        [ForeignKey(nameof(CategoryId))]
        public ProductCategories ProductCategories { get; set; } = null!;
    }
}
