using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class ProductFlowers : BaseEntity
    {
        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required]
        [ForeignKey("Flower")]
        public int FlowerId { get; set; }

        [Required]
        public int QuantityNeeded { get; set; }

        // Navigation properties
        public virtual Products? Product { get; set; }
        public virtual Flowers Flower { get; set; }
    }
}
