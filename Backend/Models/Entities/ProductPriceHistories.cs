using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class ProductPriceHistories : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Navigation
        [ForeignKey(nameof(ProductId))]
        public Products? Products { get; set; }
    }
}
