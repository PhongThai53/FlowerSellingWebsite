using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Flower : BaseEntity
    {
        [Required]
        [MaxLength(150)]
        public required string Name { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public Guid SupplierId { get; set; }

        // Navigation Properties
        public virtual Supplier? Supplier { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<FlowerBatch> FlowerBatches { get; set; } = new List<FlowerBatch>();
        public virtual ICollection<ProductFlower> ProductFlowers { get; set; } = new List<ProductFlower>();
    }
}
