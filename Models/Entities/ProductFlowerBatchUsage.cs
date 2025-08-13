namespace FlowerSellingWebsite.Models.Entities
{
    public class ProductFlowerBatchUsage : BaseEntity
    {
        public Guid ProductId { get; set; }

        public Guid FlowerBatchDetailId { get; set; }

        public int QuantityUsed { get; set; }

        // Navigation Properties
        public virtual Product? Product { get; set; }
        public virtual FlowerBatchDetail? FlowerBatchDetail { get; set; }
    }
}
