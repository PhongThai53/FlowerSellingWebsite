using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerBatch : BaseEntity
    {
        public Guid SupplierId { get; set; }

        public Guid FlowerId { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }

        public decimal TotalAmount { get; set; }

        // Navigation Properties
        public virtual Supplier? Supplier { get; set; }
        public virtual Flower? Flower { get; set; }
        public virtual ICollection<FlowerBatchDetail> FlowerBatchDetails { get; set; } = new List<FlowerBatchDetail>();
    }
}
