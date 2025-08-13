using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Product : BaseEntity
    {
        [MaxLength(150)]
        public string? Name { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }

        public decimal Price { get; set; }

        // Navigation Properties
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<ProductFlower> ProductFlowers { get; set; } = new List<ProductFlower>();
        public virtual ICollection<ProductFlowerBatchUsage> ProductFlowerBatchUsages { get; set; } = new List<ProductFlowerBatchUsage>();
    }
}
