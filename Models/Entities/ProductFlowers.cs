using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    [Table("ProductFlowers")]
    public class ProductFlower : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int FlowerId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "QuantityNeeded must be positive")]
        public int QuantityNeeded { get; set; }

        public bool IsRequired { get; set; } = true;

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("FlowerId")]
        public virtual Flower Flower { get; set; } = null!;
    }
}
